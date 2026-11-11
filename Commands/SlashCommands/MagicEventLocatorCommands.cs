using Coaction.KickAssCardBot.Manager;
using Discord;
using Discord.Interactions;

namespace Coaction.KickAssCardBot.Commands.SlashCommands
{
    public class MagicEventLocatorCommands : InteractionModuleBase<SocketInteractionContext>
    {
        private readonly WizardsEventLocatorManager _wizardsEventLocatorManager;

        public MagicEventLocatorCommands(WizardsEventLocatorManager wizardsEventLocatorManager)
        {
            _wizardsEventLocatorManager = wizardsEventLocatorManager;
        }

        [SlashCommand("event-locator", "Uses Wizards event locator to locate events that aren't FNM level.")]
        public async Task GetMagicEvents(string zipCode)
        {

            var events = await _wizardsEventLocatorManager.GetEventLocationByZipcode(zipCode);
            if (events is { Count: > 0 })
            {
                var optionBuilder = events.Select(magicEvent => new SelectMenuOptionBuilder { Label = $"{magicEvent.Name}", Description = magicEvent.Format.ToLower(), Value = $"magicevent-{magicEvent.EventId}" }).ToList();
                var componentBuilder = new ComponentBuilder();
                componentBuilder.WithSelectMenu($"magicevent-{zipCode}", optionBuilder, "Events");
                var embedBuilder = new EmbedBuilder
                {
                    Title = $"Finding Magic Events Near {zipCode}",
                    Description = $"Select an event in the drop down list for more details.",
                    Color = Color.DarkTeal,
                    Url = "https://locator.wizards.com/"
                };

                await Context.Interaction.RespondAsync(embed: embedBuilder.Build(),
                    components: componentBuilder.Build());
            }
            else
            {
                await Context.Interaction.Channel.SendMessageAsync($"Failed to find events around {zipCode}. There aren't any registered or the zip code isn't valid.");
            }
        }
    }
}
