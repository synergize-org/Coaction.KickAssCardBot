namespace Coaction.KickAssCardBot.Models.Spoiler
{
    public class DiscordSpoilerConfig
    {
        public ulong GuildId { get; set; }
        public ulong ChannelId { get; set; }
        public bool Enabled { get; set; }
        public DateTime? LastCheckedUtc { get; set; }
        public DateTime? LastUpdatedUtc { get; set; }
        public string? LastPostedCardId { get; set; }
    }
}
