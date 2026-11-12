namespace Coaction.KickAssCardBot.Models.Spoiler
{
    public class QueuedSpoilerCard
    {
        public ulong GuildId { get; set; }
        public ulong ChannelId { get; set; }
        public ulong ThreadId { get; set; }
        public string CardId { get; set; } = string.Empty;
        public string CardName { get; set; } = string.Empty;
        public string SetName { get; set; } = string.Empty;
        public DateTime ReleasedAt { get; set; }
        public DateTime EnqueuedAtUtc { get; set; }
    }
}