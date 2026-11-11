using Newtonsoft.Json;

namespace Coaction.KickAssCardBot.Models.MagicEventLocator
{
    public class Distributor
    {
        [JsonProperty("name")]
        public string Name { get; set; }
    }
}
