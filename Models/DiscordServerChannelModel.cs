using Newtonsoft.Json;

namespace Coaction.KickAssCardBot.Models
{
    public class DiscordServerChannelModel
    {
        public ulong ServerId { get; set; }
        public ulong ChannelId { get; set; }

        [JsonProperty("ConfiguredFormats", NullValueHandling = NullValueHandling.Ignore)]
        public List<string> ListOfFormats { get; set; }

        public DiscordServerChannelModel(ulong serverid, ulong channelid)
        {
            ServerId = serverid;
            ChannelId = channelid;
        }

        public DiscordServerChannelModel()
        {

        }
    }
}
