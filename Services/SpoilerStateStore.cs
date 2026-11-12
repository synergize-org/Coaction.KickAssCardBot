using Coaction.KickAssCardBot.Models.Spoiler;
using System.Collections.Concurrent;
using System.Text.Json;

namespace Coaction.KickAssCardBot.Services
{
    public class SpoilerStateStore
    {
        private readonly string _filePath;
        private readonly object _stateLock = new();
        private readonly ConcurrentQueue<QueuedSpoilerCard> _queue;
        private readonly SemaphoreSlim _queueSignal;

        public SpoilerStateStore()
        {
            var configuredDirectory = Environment.GetEnvironmentVariable("SPOILER_DATA_PATH");

            var directory = !string.IsNullOrWhiteSpace(configuredDirectory)
                ? configuredDirectory
                : OperatingSystem.IsWindows()
                    ? Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), "KickAssCardBot", "spoiler-data")
                    : Path.Combine("/app", "Data", "spoiler-data");

            Directory.CreateDirectory(directory);
            _filePath = Path.Combine(directory, "state.json");
            EnsureInitialized();
            _queue = new ConcurrentQueue<QueuedSpoilerCard>(LoadState().QueuedCards.Select(CopyQueuedCard));
            _queueSignal = new SemaphoreSlim(_queue.Count);
        }

        public DiscordSpoilerConfig? GetGuildConfig(ulong guildId)
        {
            lock (_stateLock)
            {
                return LoadState().GuildConfigs.FirstOrDefault(x => x.GuildId == guildId);
            }
        }

        public IReadOnlyList<DiscordSpoilerConfig> GetEnabledGuildConfigs()
        {
            lock (_stateLock)
            {
                return LoadState().GuildConfigs
                    .Where(x => x.Enabled && x.ChannelId > 0)
                    .Select(CopyConfig)
                    .ToList();
            }
        }

        public void SaveGuildConfig(DiscordSpoilerConfig config)
        {
            lock (_stateLock)
            {
                var state = LoadState();
                var existing = state.GuildConfigs.FirstOrDefault(x => x.GuildId == config.GuildId);
                if (existing == null)
                {
                    state.GuildConfigs.Add(CopyConfig(config));
                }
                else
                {
                    if (existing.ChannelId != config.ChannelId || !config.Enabled)
                    {
                        state.QueuedCards.RemoveAll(x => x.GuildId == config.GuildId);
                    }

                    existing.ChannelId = config.ChannelId;
                    existing.Enabled = config.Enabled;
                    existing.LastCheckedUtc = config.LastCheckedUtc;
                    existing.LastUpdatedUtc = DateTime.UtcNow;
                    existing.LastPostedCardId = config.LastPostedCardId;
                }

                SaveState(state);
            }
        }

        public bool HasPostedCard(ulong guildId, string cardId)
        {
            lock (_stateLock)
            {
                return LoadState().PostedCards.Any(x => x.GuildId == guildId && CardIdMatches(x.CardId, cardId));
            }
        }

        public bool HasQueuedCard(ulong guildId, string cardId)
        {
            lock (_stateLock)
            {
                return LoadState().QueuedCards.Any(x => x.GuildId == guildId && CardIdMatches(x.CardId, cardId));
            }
        }

        public ulong? GetSetThreadId(ulong guildId, ulong channelId, string setName)
        {
            lock (_stateLock)
            {
                return LoadState().SetThreads
                    .FirstOrDefault(x => x.GuildId == guildId &&
                                         x.ChannelId == channelId &&
                                         string.Equals(x.SetName, setName, StringComparison.OrdinalIgnoreCase))
                    ?.ThreadId;
            }
        }

        public void SaveSetThread(SpoilerSetThread thread)
        {
            lock (_stateLock)
            {
                var state = LoadState();
                var existing = state.SetThreads.FirstOrDefault(x =>
                    x.GuildId == thread.GuildId &&
                    x.ChannelId == thread.ChannelId &&
                    string.Equals(x.SetName, thread.SetName, StringComparison.OrdinalIgnoreCase));

                if (existing == null)
                {
                    state.SetThreads.Add(thread);
                }
                else
                {
                    existing.ThreadId = thread.ThreadId;
                }

                foreach (var queuedCard in state.QueuedCards.Where(x =>
                             x.GuildId == thread.GuildId &&
                             x.ChannelId == thread.ChannelId &&
                             string.Equals(x.SetName, thread.SetName, StringComparison.OrdinalIgnoreCase)))
                {
                    queuedCard.ThreadId = thread.ThreadId;
                }

                SaveState(state);
            }
        }

        public void UpdateQueuedCardRouting(QueuedSpoilerCard card, string setName, ulong threadId)
        {
            lock (_stateLock)
            {
                var state = LoadState();
                var queuedCard = state.QueuedCards.FirstOrDefault(x =>
                    x.GuildId == card.GuildId && CardIdMatches(x.CardId, card.CardId));
                if (queuedCard != null)
                {
                    queuedCard.SetName = setName;
                    queuedCard.ThreadId = threadId;
                    SaveState(state);
                }

                card.SetName = setName;
                card.ThreadId = threadId;
            }
        }

        public void EnqueueCards(IEnumerable<QueuedSpoilerCard> cards)
        {
            lock (_stateLock)
            {
                var state = LoadState();
                var cardsToEnqueue = new List<QueuedSpoilerCard>();
                foreach (var card in cards)
                {
                    var alreadyPosted = state.PostedCards.Any(x => x.GuildId == card.GuildId && CardIdMatches(x.CardId, card.CardId));
                    var alreadyQueued = state.QueuedCards.Any(x => x.GuildId == card.GuildId && CardIdMatches(x.CardId, card.CardId));
                    if (!alreadyPosted && !alreadyQueued)
                    {
                        var queuedCard = CopyQueuedCard(card);
                        state.QueuedCards.Add(queuedCard);
                        cardsToEnqueue.Add(queuedCard);
                    }
                }

                SaveState(state);
                foreach (var card in cardsToEnqueue)
                {
                    _queue.Enqueue(CopyQueuedCard(card));
                    _queueSignal.Release();
                }
            }
        }

        public async Task<QueuedSpoilerCard> WaitForQueuedCardAsync(CancellationToken cancellationToken)
        {
            while (true)
            {
                await _queueSignal.WaitAsync(cancellationToken);
                if (TryDequeueQueuedCard(out var card) && card != null)
                {
                    return card;
                }
            }
        }

        public bool TryDequeueQueuedCard(out QueuedSpoilerCard? card)
        {
            lock (_stateLock)
            {
                var state = LoadState();
                while (_queue.TryDequeue(out var next))
                {
                    var persistedCard = state.QueuedCards.FirstOrDefault(x =>
                        x.GuildId == next.GuildId && CardIdMatches(x.CardId, next.CardId));
                    if (persistedCard != null)
                    {
                        card = CopyQueuedCard(persistedCard);
                        return true;
                    }
                }

                card = null;
                return false;
            }
        }

        public void RecordQueuedCardPosted(QueuedSpoilerCard card)
        {
            lock (_stateLock)
            {
                var state = LoadState();
                state.QueuedCards.RemoveAll(x => x.GuildId == card.GuildId && CardIdMatches(x.CardId, card.CardId));

                if (!state.PostedCards.Any(x => x.GuildId == card.GuildId && CardIdMatches(x.CardId, card.CardId)))
                {
                    state.PostedCards.Add(new PostedSpoilerCard
                    {
                        GuildId = card.GuildId,
                        CardId = card.CardId,
                        CardName = card.CardName,
                        PostedAtUtc = DateTime.UtcNow
                    });
                }

                UpdateLastPostedCard(state, card.GuildId, card.CardId);
                SaveState(state);
            }
        }

        public void DiscardQueuedCard(QueuedSpoilerCard card)
        {
            lock (_stateLock)
            {
                var state = LoadState();
                var removedCount = state.QueuedCards.RemoveAll(x => x.GuildId == card.GuildId && CardIdMatches(x.CardId, card.CardId));
                if (removedCount > 0)
                {
                    SaveState(state);
                }
            }
        }

        public void ReturnQueuedCardToEnd(QueuedSpoilerCard card)
        {
            lock (_stateLock)
            {
                var state = LoadState();
                var queuedCard = state.QueuedCards.FirstOrDefault(x => x.GuildId == card.GuildId && CardIdMatches(x.CardId, card.CardId));
                if (queuedCard != null)
                {
                    state.QueuedCards.Remove(queuedCard);
                    state.QueuedCards.Add(queuedCard);
                    SaveState(state);
                    _queue.Enqueue(CopyQueuedCard(queuedCard));
                    _queueSignal.Release();
                }
            }
        }

        public void RecordPostedCard(ulong guildId, string cardId, string cardName)
        {
            lock (_stateLock)
            {
                var state = LoadState();
                if (state.PostedCards.Any(x => x.GuildId == guildId && CardIdMatches(x.CardId, cardId)))
                {
                    return;
                }

                state.PostedCards.Add(new PostedSpoilerCard
                {
                    GuildId = guildId,
                    CardId = cardId,
                    CardName = cardName,
                    PostedAtUtc = DateTime.UtcNow
                });

                UpdateLastPostedCard(state, guildId, cardId);

                SaveState(state);
            }
        }

        public void UpdateLastChecked(ulong guildId)
        {
            lock (_stateLock)
            {
                var state = LoadState();
                var config = state.GuildConfigs.FirstOrDefault(x => x.GuildId == guildId);
                if (config != null)
                {
                    config.LastCheckedUtc = DateTime.UtcNow;
                    config.LastUpdatedUtc = DateTime.UtcNow;
                    SaveState(state);
                }
            }
        }

        private SpoilerPersistenceState LoadState()
        {
            if (!File.Exists(_filePath))
            {
                return new SpoilerPersistenceState();
            }

            var content = File.ReadAllText(_filePath);
            if (string.IsNullOrWhiteSpace(content))
            {
                return new SpoilerPersistenceState();
            }

            return JsonSerializer.Deserialize<SpoilerPersistenceState>(content) ?? new SpoilerPersistenceState();
        }

        private void SaveState(SpoilerPersistenceState state)
        {
            var tempPath = $"{_filePath}.tmp";
            var json = JsonSerializer.Serialize(state, new JsonSerializerOptions { WriteIndented = true });
            File.WriteAllText(tempPath, json);
            File.Move(tempPath, _filePath, overwrite: true);
        }

        private static bool CardIdMatches(string left, string right) =>
            string.Equals(left, right, StringComparison.OrdinalIgnoreCase);

        private static void UpdateLastPostedCard(SpoilerPersistenceState state, ulong guildId, string cardId)
        {
            var config = state.GuildConfigs.FirstOrDefault(x => x.GuildId == guildId);
            if (config != null)
            {
                config.LastPostedCardId = cardId;
                config.LastUpdatedUtc = DateTime.UtcNow;
            }
        }

        private static DiscordSpoilerConfig CopyConfig(DiscordSpoilerConfig config) => new()
        {
            GuildId = config.GuildId,
            ChannelId = config.ChannelId,
            Enabled = config.Enabled,
            LastCheckedUtc = config.LastCheckedUtc,
            LastUpdatedUtc = config.LastUpdatedUtc,
            LastPostedCardId = config.LastPostedCardId
        };

        private static QueuedSpoilerCard CopyQueuedCard(QueuedSpoilerCard card) => new()
        {
            GuildId = card.GuildId,
            ChannelId = card.ChannelId,
            ThreadId = card.ThreadId,
            CardId = card.CardId,
            CardName = card.CardName,
            SetName = card.SetName,
            ReleasedAt = card.ReleasedAt,
            EnqueuedAtUtc = card.EnqueuedAtUtc
        };

        private void EnsureInitialized()
        {
            if (File.Exists(_filePath))
            {
                return;
            }

            SaveState(new SpoilerPersistenceState());
        }
    }
}
