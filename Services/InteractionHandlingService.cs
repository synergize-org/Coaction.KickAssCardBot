using Coaction.KickAssCardBot.Embed_Output;
using Coaction.KickAssCardBot.Extensions;
using Coaction.KickAssCardBot.Helpers;
using Coaction.KickAssCardBot.Manager;
using Coaction.KickAssCardBot.Models.Scryfall;
using Discord;
using Discord.Interactions;
using Discord.WebSocket;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using System.Reflection;

namespace Coaction.KickAssCardBot.Services
{
    public class InteractionHandlingService
    {
        private readonly DiscordSocketClient _client;
        private readonly InteractionService _interactionService;
        private readonly IServiceProvider _serviceProvider;
        private readonly ILogger<InteractionHandlingService> _logger;
        private readonly ScryfallManagerService _scryfallManagerService;
        private readonly MtgCardOutputManager _mtgCardOutputManager;
        private readonly WizardsEventLocatorManager _wizardsEventLocatorManager;
        private readonly MtgMeleeManager _mtgMeleeManager;
        private readonly IConfiguration _configuration;

        public InteractionHandlingService(ILogger<InteractionHandlingService> logger, DiscordSocketClient client, InteractionService interactionService,
            IServiceProvider serviceProvider, ScryfallManagerService scryfallManagerService, MtgCardOutputManager mtgCardOutputManager, WizardsEventLocatorManager wizardsEventLocatorManager, MtgMeleeManager mtgMeleeManager, IConfiguration configurationManager)
        {
            _configuration = configurationManager;
            _client = client;
            _interactionService = interactionService;
            _serviceProvider = serviceProvider;
            _scryfallManagerService = scryfallManagerService;
            _mtgCardOutputManager = mtgCardOutputManager;
            _wizardsEventLocatorManager = wizardsEventLocatorManager;
            _logger = logger;
            _mtgMeleeManager = mtgMeleeManager;

            _client.Ready += ClientOnReadyAsync;
        }

        public async Task InitializeAsync()
        {
            await _interactionService.AddModulesAsync(Assembly.GetEntryAssembly(), _serviceProvider);
        }

        /// <summary>
        /// Executes when discord bot is ready. The if check here is to ensure we don't see duplicate commands when the discord both is live. 
        /// </summary>
        /// <returns></returns>
        public async Task ClientOnReadyAsync()
        {
            #if DEBUG
            _logger.LogDebug("Bot is in debug mode. Registering commands to test discord for immediate testability.");
            var discordServerId = _configuration["TEST_DISCORD_SERVER_ID"];
            if (!string.IsNullOrEmpty(discordServerId))
            {
                if (ulong.TryParse(discordServerId, out var parsedId))
                {
                    await _interactionService.RegisterCommandsToGuildAsync(parsedId);

                }
                else
                {
                    _logger.LogWarning($"Failed to obtain test server ID. Double check to make sure environment variable is configured.");
                }
            };
            #else
            await _interactionService.RegisterCommandsGloballyAsync();
            #endif
        }

        public async Task HandleInteractionAsync(SocketInteraction arg)
        {
            try
            {
                var context = new SocketInteractionContext(_client, arg);
                await _interactionService.ExecuteCommandAsync(context, _serviceProvider);
            }
            catch (Exception e)
            {
                _logger.LogError($"Failed to handle Interaction due to: {e.Message}");
            }
        }

        public async Task Client_SelectMenuExecuted(SocketMessageComponent arg)
        {
            if (arg.Data.CustomId.Contains("cardset-"))
            {
                var cardId = arg.Data.CustomId.Replace("cardset-", "");
                var cardInfo = await _scryfallManagerService.PullScryfallData(cardId, arg.Data.Values.FirstOrDefault());
                var purchaseCardButtons = ButtonComponentHelper.BuildBuyButtons(cardInfo, false);
                await arg.RespondAsync(embed: _mtgCardOutputManager.CardOutput(cardInfo).Build(), components: purchaseCardButtons.Build(), ephemeral: true);
            }

            if (arg.Data.CustomId.Contains("didyoumean-"))
            {
                var authorId = ulong.Parse(arg.Data.CustomId.Replace("didyoumean-", ""));
                if (arg.User.Id == authorId)
                {
                    var cardInfo = await _scryfallManagerService.PullScryfallData(arg.Data.Values.FirstOrDefault());
                    var scryFallSetData = await _scryfallManagerService.PullScryfallSetData(cardInfo?.PrintsSearchUri);
                    var purchaseCardButtons = ButtonComponentHelper.BuildBuyButtons(cardInfo, selectItems: scryFallSetData.BuildPrintingSelectMenu());
                    await arg.UpdateAsync(properties =>
                    {
                        properties.Components = purchaseCardButtons.Build();
                        properties.Embed = _mtgCardOutputManager.CardOutput(cardInfo).Build();
                    });
                }
                else
                {
                    await arg.DeferAsync(true);
                }
            }

            if (arg.Data.CustomId.Contains($"magicevent-"))
            {
                try
                {
                    var eventId = arg.Data.Values.First().Replace($"magicevent-", "");
                    var magicEvent = await _wizardsEventLocatorManager.GetMagicEvent(eventId);
                    if (magicEvent != null)
                    {
                        var embed = new EmbedBuilder
                        {
                            Title = magicEvent.Name,
                            Description = $"{magicEvent.Description.Replace("\n", "")} \n ```{magicEvent.Store.PostalAddress.Replace("\n", " ")}```",
                            Url = $"https://locator.wizards.com/events/{eventId}",
                            Color = Color.DarkGreen,
                            Timestamp = magicEvent.StartDatetime,
                            Footer = new EmbedFooterBuilder
                            {
                                Text = "Event Date"
                            }
                        };

                        var formattedFormat = magicEvent.Format.ToLower();
                        embed.AddField($"{nameof(magicEvent.Distance)}", $"{Math.Round(magicEvent.Distance.ConvertToMiles(), 2)} Miles", true);
                        embed.AddField($"{nameof(magicEvent.Format)}", $"{char.ToUpper(formattedFormat[0])}{formattedFormat[1..]}", true);
                        embed.AddField($"{nameof(magicEvent.Store.Name)}", magicEvent.Store.Name, true);
                        embed.AddField($"{nameof(magicEvent.Currency)}", $"${magicEvent.Cost * 0.01} {magicEvent.Currency}", true);
                        embed.AddField($"{nameof(magicEvent.Store.PhoneNumber)}", magicEvent.Store.PhoneNumber, true);
                        embed.AddField($"{nameof(magicEvent.Store.Website)}", magicEvent.Store.Website, true);
                        await arg.RespondAsync(embed: embed.Build(), ephemeral: true);
                    }
                }
                catch (Exception e)
                {
                    _logger.LogError(e, $"Failed to find event details from selection menu due to {e.Message}");
                    throw;
                }
            }

            if (arg.Data.CustomId.Contains("mtgmelee-tournament-"))
            {
                try
                {
                    var splitValues = arg.Data.Values.FirstOrDefault()?.Split("-");
                    var tournmanetId = splitValues?[1];
                    var roundId = splitValues?[2];
                    var roundData = await _mtgMeleeManager.GetTournamentResults(tournmanetId);
                    var componentBuilder = new ComponentBuilder();
                    componentBuilder.WithSelectMenu($"mtgmelee-tournament-{tournmanetId}", roundData.BuildRoundSelectionMenu());

                    await arg.RespondAsync(embed: MtgMeleeOutput.OutputDecklistAggregator(roundData, roundId).Build(), components: componentBuilder.Build(), ephemeral: true);
                }
                catch (Exception e)
                {
                    _logger.LogError($"Failed to select round for tournament due to {e.Message}");
                    throw;
                }
            }
        }

        public async Task<Task> Client_ButtonExecuted(SocketMessageComponent component)
        {
            var customId = component.Data.CustomId;
            if (customId.Contains("rules-"))
            {
                var guidFromComponent = Guid.Parse(customId.Replace("rules-", ""));
                var cardData = await _scryfallManagerService.PullScryfallData(guidFromComponent);
                var cardRules = await _scryfallManagerService.PullScryFallRuleData(guidFromComponent);
                var firstRule = cardRules.Rules.FirstOrDefault();
                await component.RespondAsync(embed: _mtgCardOutputManager.RulingOutput(cardRules, cardData, firstRule).Build(), components: ButtonComponentHelper.BuildRuleButtons(cardRules, firstRule, cardData.Id).Build(), ephemeral: true);
            }

            // Logic setup to handle the pagination of Rules when "Next" or "Previous" buttons are pressed on a ruling output.
            if (customId.Contains("CardRule-"))
            {
                var indexCharacterStart = customId.IndexOf('#');
                var index = customId.Substring(indexCharacterStart + 1);
                if (int.TryParse(index, out var parsedIndex))
                {
                    var oracleId = Guid.Parse(component.Data.CustomId.Replace("CardRule-Next-", "").Replace("CardRule-Previous-", "").Replace("CardRule-Send-", "").Replace(customId.Substring(indexCharacterStart), ""));
                    var rules = await _scryfallManagerService.PullScryFallRuleData(oracleId);
                    var cardData = await _scryfallManagerService.PullScryfallData(oracleId);
                    if (customId.Contains("CardRule-Send-"))
                    {
                        await component.RespondAsync(embed: _mtgCardOutputManager.RulingOutput(rules, cardData, rules.Rules[parsedIndex]).Build());
                        return Task.CompletedTask;
                    }
                    ScryFallCardRulingsModel.Rule rule = null;

                    if (customId.Contains("Next"))
                    {
                        rule = rules.Rules[parsedIndex + 1];
                    }

                    if (customId.Contains("Previous"))
                    {
                        rule = rules.Rules[parsedIndex - 1];
                    }

                    await component.RespondAsync(
                        embed: _mtgCardOutputManager.RulingOutput(rules, cardData, rule).Build(),
                        components: ButtonComponentHelper.BuildRuleButtons(rules, rule, cardData.Id).Build(), ephemeral: true);

                    return Task.CompletedTask;
                }
            }

            if (customId.Contains($"legalities-"))
            {

                var guidFromComponent = Guid.Parse(customId.Replace("legalities-", ""));
                var cardData = await _scryfallManagerService.PullScryfallData(guidFromComponent);
                await component.RespondAsync(embed: _mtgCardOutputManager.LegalitiesOutput(cardData).Build(), ephemeral: true);
            }

            return Task.CompletedTask;
        }
    }
}
