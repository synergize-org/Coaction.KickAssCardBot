namespace Coaction.KickAssCardBot.Models.Spoiler
{
    public class SpoilerSetThread
    {
        public ulong GuildId { get; set; }
        public ulong ChannelId { get; set; }
        public string SetName { get; set; } = string.Empty;
        public ulong ThreadId { get; set; }
    }
}