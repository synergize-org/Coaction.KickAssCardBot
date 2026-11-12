using Coaction.KickAssCardBot.Models.Spoiler;
using Coaction.KickAssCardBot.Services;
using Discord;
using Discord.Interactions;

namespace Coaction.KickAssCardBot.Commands.SlashCommands
{
    public class SpoilerConfigSlashCommands : InteractionModuleBase<SocketInteractionContext>
    {
        private readonly SpoilerStateStore _spoilerStateStore;

        public SpoilerConfigSlashCommands(SpoilerStateStore spoilerStateStore)
        {
            _spoilerStateStore = spoilerStateStore;
        }

        [SlashCommand("spoiler-config", "Enable or disable spoiler posting and choose the channel to post spoilers to.")]
        [RequireUserPermission(GuildPermission.ManageChannels)]
        public async Task ConfigureSpoilers(
            [Summary("enabled", "Whether spoiler posting is enabled for this server.")] bool enabled = true,
            [Summary("channel", "The text channel to post spoiler cards to.")] ITextChannel? channel = null)
        {
            if (Context.Guild == null)
            {
                await RespondAsync("This command must be used in a server.", ephemeral: true);
                return;
            }

            if (enabled && channel == null)
            {
                await RespondAsync("You must provide a channel when enabling spoiler posting.", ephemeral: true);
                return;
            }

            var config = _spoilerStateStore.GetGuildConfig(Context.Guild.Id) ?? new DiscordSpoilerConfig
            {
                GuildId = Context.Guild.Id,
                Enabled = enabled,
                ChannelId = channel?.Id ?? 0,
                LastUpdatedUtc = DateTime.UtcNow
            };

            config.Enabled = enabled;
            config.LastUpdatedUtc = DateTime.UtcNow;

            if (channel != null)
            {
                config.ChannelId = channel.Id;
            }

            if (!enabled)
            {
                config.LastCheckedUtc = DateTime.UtcNow;
            }

            _spoilerStateStore.SaveGuildConfig(config);

            var response = enabled
                ? $"Spoiler posting is enabled and cards will be posted in {channel.Mention}."
                : "Spoiler posting is now disabled for this server.";

            await RespondAsync(response, ephemeral: true);
        }
    }
}
