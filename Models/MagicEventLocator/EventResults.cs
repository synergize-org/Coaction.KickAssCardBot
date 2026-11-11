using Newtonsoft.Json;

namespace Coaction.KickAssCardBot.Models.MagicEventLocator
{
    internal class EventResults
    {
        [JsonProperty("page")]
        public int Page { get; set; }

        [JsonProperty("pageSize")]
        public int PageSize { get; set; }

        [JsonProperty("totalResults")]
        public int TotalResults { get; set; }

        [JsonProperty("results")]
        public List<Event> Results { get; set; }
    }
}
