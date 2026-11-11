namespace Coaction.KickAssCardBot.Models.Spoiler
{
    public class SpoilerPersistenceState
    {
        public List<DiscordSpoilerConfig> GuildConfigs { get; set; } = new();
        public List<PostedSpoilerCard> PostedCards { get; set; } = new();
    }
}
