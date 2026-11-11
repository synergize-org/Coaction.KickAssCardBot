using Newtonsoft.Json;

namespace Coaction.KickAssCardBot.Models.GeoLocation
{
    public class Bounds
    {
        [JsonProperty("northeast")]
        public Northeast Northeast { get; set; }

        [JsonProperty("southwest")]
        public Southwest Southwest { get; set; }
    }
}
