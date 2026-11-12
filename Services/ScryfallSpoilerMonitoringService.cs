using Coaction.KickAssCardBot.Embed_Output;
using Coaction.KickAssCardBot.Manager;
using Coaction.KickAssCardBot.Models.Spoiler;
using Discord;
using Discord.WebSocket;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace Coaction.KickAssCardBot.Services
{
    public class ScryfallSpoilerMonitoringService : BackgroundService
    {
        private readonly ILogger<ScryfallSpoilerMonitoringService> _logger;
        private readonly DiscordSocketClient _client;
        private readonly IServiceScopeFactory _scopeFactory;
        private readonly MtgCardOutputManager _mtgCardOutputManager;
        private readonly SpoilerStateStore _spoilerStateStore;
        private readonly TaskCompletionSource _clientReady = new(TaskCreationOptions.RunContinuationsAsynchronously);
        private readonly TimeSpan _pollInterval = TimeSpan.FromMinutes(5);
        private readonly TimeSpan _retryDelay = TimeSpan.FromSeconds(1);

        public ScryfallSpoilerMonitoringService(
            ILogger<ScryfallSpoilerMonitoringService> logger,
            DiscordSocketClient client,
            IServiceScopeFactory scopeFactory,
            MtgCardOutputManager mtgCardOutputManager,
            SpoilerStateStore spoilerStateStore)
        {
            _logger = logger;
            _client = client;
            _scopeFactory = scopeFactory;
            _mtgCardOutputManager = mtgCardOutputManager;
            _spoilerStateStore = spoilerStateStore;
            _client.Ready += OnClientReadyAsync;
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            try
            {
                await _clientReady.Task.WaitAsync(stoppingToken);
                await Task.WhenAll(PollForSpoilersAsync(stoppingToken), SendQueuedCardsAsync(stoppingToken));
            }
            catch (OperationCanceledException) when (stoppingToken.IsCancellationRequested)
            {
            }
            finally
            {
                _client.Ready -= OnClientReadyAsync;
            }
        }

        private Task OnClientReadyAsync()
        {
            _clientReady.TrySetResult();
            return Task.CompletedTask;
        }

        private async Task PollForSpoilersAsync(CancellationToken stoppingToken)
        {
            while (!stoppingToken.IsCancellationRequested)
            {
                try
                {
                    await ProcessSpoilersAsync();
                }
                catch (OperationCanceledException) when (stoppingToken.IsCancellationRequested)
                {
                    break;
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Failed to process spoiler cards.");
                }

                try
                {
                    await Task.Delay(_pollInterval, stoppingToken);
                }
                catch (OperationCanceledException) when (stoppingToken.IsCancellationRequested)
                {
                    break;
                }
            }
        }

        private async Task ProcessSpoilersAsync()
        {
            var enabledConfigs = _spoilerStateStore.GetEnabledGuildConfigs();
            if (enabledConfigs.Count == 0)
            {
                return;
            }

            using var scope = _scopeFactory.CreateScope();
            var scryfallManagerService = scope.ServiceProvider.GetRequiredService<ScryfallManagerService>();
            var recentSpoilers = await scryfallManagerService.PullRecentSpoilers();
            if (recentSpoilers.Count == 0)
            {
                return;
            }

            foreach (var config in enabledConfigs)
            {
                var guild = _client.GetGuild(config.GuildId);
                if (guild == null)
                {
                    continue;
                }

                var channel = guild.GetTextChannel(config.ChannelId);
                if (channel == null)
                {
                    continue;
                }

                var newSpoilers = recentSpoilers
                    .Where(x => Guid.TryParse(x.Id, out _) &&
                                !string.IsNullOrWhiteSpace(x.SetName) &&
                                !_spoilerStateStore.HasPostedCard(config.GuildId, x.Id) &&
                                !_spoilerStateStore.HasQueuedCard(config.GuildId, x.Id) &&
                                x.ReleasedAt >= DateTime.UtcNow.AddDays(-7)) // Only consider spoilers released in the last 7 days)
                    .GroupBy(x => x.SetName.Trim(), StringComparer.OrdinalIgnoreCase)
                    .OrderBy(x => x.Key, StringComparer.OrdinalIgnoreCase);

                foreach (var setGroup in newSpoilers)
                {
                    try
                    {
                        var thread = await GetOrCreateSetThreadAsync(guild, channel, setGroup.Key);
                        var queuedCards = setGroup.Select(spoiler => new QueuedSpoilerCard
                        {
                            GuildId = config.GuildId,
                            ChannelId = channel.Id,
                            ThreadId = thread.Id,
                            CardId = spoiler.Id,
                            CardName = spoiler.Name,
                            SetName = setGroup.Key,
                            ReleasedAt = spoiler.ReleasedAt,
                            EnqueuedAtUtc = DateTime.UtcNow
                        });

                        _spoilerStateStore.EnqueueCards(queuedCards);
                    }
                    catch (Exception ex)
                    {
                        _logger.LogError(ex, "Failed to create a spoiler thread for set {SetName} in {GuildId}:{ChannelId}", setGroup.Key, config.GuildId, config.ChannelId);
                    }
                }

                _spoilerStateStore.UpdateLastChecked(config.GuildId);
            }
        }

        private async Task<IThreadChannel> GetOrCreateSetThreadAsync(SocketGuild guild, SocketTextChannel channel, string setName)
        {
            var savedThreadId = _spoilerStateStore.GetSetThreadId(guild.Id, channel.Id, setName);
            if (savedThreadId.HasValue && guild.GetThreadChannel(savedThreadId.Value) is { IsArchived: false } existingThread)
            {
                return existingThread;
            }

            var threadTitle = $"Spoilers for {setName} released";
            var starterMessage = await channel.SendMessageAsync(threadTitle);
            var thread = await channel.CreateThreadAsync(
                threadTitle,
                ThreadType.PublicThread,
                ThreadArchiveDuration.OneWeek,
                starterMessage);
            _spoilerStateStore.SaveSetThread(new SpoilerSetThread
            {
                GuildId = guild.Id,
                ChannelId = channel.Id,
                SetName = setName,
                ThreadId = thread.Id
            });

            _logger.LogInformation("Created spoiler thread {ThreadName} in {GuildName}", threadTitle, guild.Name);
            return thread;
        }

        private async Task SendQueuedCardsAsync(CancellationToken stoppingToken)
        {
            while (!stoppingToken.IsCancellationRequested)
            {
                QueuedSpoilerCard? queuedCard;
                try
                {
                    queuedCard = await _spoilerStateStore.WaitForQueuedCardAsync(stoppingToken);
                }
                catch (OperationCanceledException) when (stoppingToken.IsCancellationRequested)
                {
                    break;
                }

                try
                {
                    if (!Guid.TryParse(queuedCard.CardId, out var cardId))
                    {
                        _logger.LogWarning("Discarding queued spoiler with invalid Scryfall ID {CardId}", queuedCard.CardId);
                        _spoilerStateStore.DiscardQueuedCard(queuedCard);
                        continue;
                    }

                    using var scope = _scopeFactory.CreateScope();
                    var scryfallManagerService = scope.ServiceProvider.GetRequiredService<ScryfallManagerService>();
                    var cardData = await scryfallManagerService.PullScryfallData(cardId);
                    var setName = string.IsNullOrWhiteSpace(queuedCard.SetName)
                        ? cardData.SetName
                        : queuedCard.SetName;
                    var guild = _client.GetGuild(queuedCard.GuildId)
                        ?? throw new InvalidOperationException($"Guild {queuedCard.GuildId} is not available.");
                    var channel = guild.GetTextChannel(queuedCard.ChannelId)
                        ?? throw new InvalidOperationException($"Spoiler channel {queuedCard.ChannelId} is not available.");
                    var setThread = await GetOrCreateSetThreadAsync(guild, channel, setName);
                    _spoilerStateStore.UpdateQueuedCardRouting(queuedCard, setName, setThread.Id);
                    await setThread.SendMessageAsync(embed: _mtgCardOutputManager.CardOutput(cardData).Build());
                    _spoilerStateStore.RecordQueuedCardPosted(queuedCard);
                    _logger.LogInformation("Posted spoiler card {CardName} to thread {ThreadId}", cardData.Name, setThread.Id);
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Failed to send queued spoiler card {CardId}; returning it to the queue", queuedCard.CardId);
                    _spoilerStateStore.ReturnQueuedCardToEnd(queuedCard);
                    try
                    {
                        await Task.Delay(_retryDelay, stoppingToken);
                    }
                    catch (OperationCanceledException) when (stoppingToken.IsCancellationRequested)
                    {
                        break;
                    }
                }
            }
        }
    }
}
