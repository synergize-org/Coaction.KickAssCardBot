namespace Coaction.KickAssCardBot.Models.Spoiler
{
    public class PostedSpoilerCard
    {
        public ulong GuildId { get; set; }
        public string CardId { get; set; } = string.Empty;
        public string CardName { get; set; } = string.Empty;
        public DateTime PostedAtUtc { get; set; }
    }
}
