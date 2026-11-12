namespace Coaction.KickAssCardBot.Models.Spoiler
{
    public class SpoilerPersistenceState
    {
        public List<DiscordSpoilerConfig> GuildConfigs { get; set; } = new();
        public List<PostedSpoilerCard> PostedCards { get; set; } = new();
        public List<QueuedSpoilerCard> QueuedCards { get; set; } = new();
        public List<SpoilerSetThread> SetThreads { get; set; } = new();
    }
}
