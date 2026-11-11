using Coaction.KickAssCardBot.Models.Spoiler;
using System.Text.Json;

namespace Coaction.KickAssCardBot.Services
{
    public class SpoilerStateStore
    {
        private readonly string _filePath;
        private readonly SemaphoreSlim _lock = new(1, 1);

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
        }

        public DiscordSpoilerConfig? GetGuildConfig(ulong guildId)
        {
            return LoadState().GuildConfigs.FirstOrDefault(x => x.GuildId == guildId);
        }

        public IReadOnlyList<DiscordSpoilerConfig> GetEnabledGuildConfigs()
        {
            return LoadState().GuildConfigs.Where(x => x.Enabled && x.ChannelId > 0).ToList();
        }

        public void SaveGuildConfig(DiscordSpoilerConfig config)
        {
            var state = LoadState();
            var existing = state.GuildConfigs.FirstOrDefault(x => x.GuildId == config.GuildId);
            if (existing == null)
            {
                state.GuildConfigs.Add(config);
            }
            else
            {
                existing.ChannelId = config.ChannelId;
                existing.Enabled = config.Enabled;
                existing.LastCheckedUtc = config.LastCheckedUtc;
                existing.LastUpdatedUtc = DateTime.UtcNow;
                existing.LastPostedCardId = config.LastPostedCardId;
            }

            SaveState(state);
        }

        public bool HasPostedCard(ulong guildId, string cardId)
        {
            return LoadState().PostedCards.Any(x => x.GuildId == guildId && string.Equals(x.CardId, cardId, StringComparison.OrdinalIgnoreCase));
        }

        public void RecordPostedCard(ulong guildId, string cardId, string cardName)
        {
            var state = LoadState();
            if (state.PostedCards.Any(x => x.GuildId == guildId && string.Equals(x.CardId, cardId, StringComparison.OrdinalIgnoreCase)))
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

            var config = state.GuildConfigs.FirstOrDefault(x => x.GuildId == guildId);
            if (config != null)
            {
                config.LastPostedCardId = cardId;
                config.LastUpdatedUtc = DateTime.UtcNow;
            }

            SaveState(state);
        }

        public void UpdateLastChecked(ulong guildId)
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
            _lock.Wait();
            try
            {
                var json = JsonSerializer.Serialize(state, new JsonSerializerOptions { WriteIndented = true });
                File.WriteAllText(_filePath, json);
            }
            finally
            {
                _lock.Release();
            }
        }

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
