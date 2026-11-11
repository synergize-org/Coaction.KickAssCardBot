using Newtonsoft.Json;

namespace Coaction.KickAssCardBot.Models.GeoLocation
{
    public class GeoLocationResults
    {
        [JsonProperty("results")]
        public List<AddressDetails> Results { get; set; }

        [JsonProperty("status")]
        public string Status { get; set; }
    }
}
