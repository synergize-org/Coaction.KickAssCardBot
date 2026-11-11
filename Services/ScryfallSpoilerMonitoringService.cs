using Coaction.KickAssCardBot.Embed_Output;
using Coaction.KickAssCardBot.Manager;
using Discord.WebSocket;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace Coaction.KickAssCardBot.Services
{
    public class ScryfallSpoilerMonitoringService : BackgroundService
    {
        private readonly ILogger<ScryfallSpoilerMonitoringService> _logger;
        private readonly DiscordSocketClient _client;
        private readonly ScryfallManagerService _scryfallManagerService;
        private readonly MtgCardOutputManager _mtgCardOutputManager;
        private readonly SpoilerStateStore _spoilerStateStore;
        private readonly TimeSpan _pollInterval = TimeSpan.FromMinutes(5);

        public ScryfallSpoilerMonitoringService(
            ILogger<ScryfallSpoilerMonitoringService> logger,
            DiscordSocketClient client,
            ScryfallManagerService scryfallManagerService,
            MtgCardOutputManager mtgCardOutputManager,
            SpoilerStateStore spoilerStateStore)
        {
            _logger = logger;
            _client = client;
            _scryfallManagerService = scryfallManagerService;
            _mtgCardOutputManager = mtgCardOutputManager;
            _spoilerStateStore = spoilerStateStore;
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            while (!stoppingToken.IsCancellationRequested)
            {
                try
                {
                    await ProcessSpoilersAsync();
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Failed to process spoiler cards.");
                }

                await Task.Delay(_pollInterval, stoppingToken);
            }
        }

        private async Task ProcessSpoilersAsync()
        {
            var enabledConfigs = _spoilerStateStore.GetEnabledGuildConfigs();
            if (enabledConfigs.Count == 0)
            {
                return;
            }

            var recentSpoilers = await _scryfallManagerService.PullRecentSpoilers();
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

                foreach (var spoiler in recentSpoilers)
                {
                    if (_spoilerStateStore.HasPostedCard(config.GuildId, spoiler.Id))
                    {
                        continue;
                    }

                    try
                    {
                        if (!Guid.TryParse(spoiler.Id, out var cardId))
                        {
                            _logger.LogWarning("Skipping spoiler card with invalid Scryfall ID {CardId}", spoiler.Id);
                            continue;
                        }

                        var cardData = await _scryfallManagerService.PullScryfallData(cardId);
                        await channel.SendMessageAsync(embed: _mtgCardOutputManager.CardOutput(cardData).Build());
                        _spoilerStateStore.RecordPostedCard(config.GuildId, spoiler.Id, cardData.Name);
                        _logger.LogInformation("Posted spoiler card {CardName} to {ChannelName} in {GuildName}", cardData.Name, channel.Name, guild.Name);
                    }
                    catch (Exception ex)
                    {
                        _logger.LogError(ex, "Failed to post spoiler card {CardId} to {GuildId}:{ChannelId}", spoiler.Id, config.GuildId, config.ChannelId);
                    }
                }

                _spoilerStateStore.UpdateLastChecked(config.GuildId);
            }
        }
    }
}
