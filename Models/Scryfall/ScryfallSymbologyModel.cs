using Newtonsoft.Json;

namespace Coaction.KickAssCardBot.Models.Scryfall
{
    public class ScryfallSymbologyModel
    {
        [JsonProperty("symbol")]
        public string Symbol { get; set; }
        [JsonProperty("svg_uri")]
        public string Svg_uri { get; set; }
    }

    public class Symbology
    {
        public List<ScryfallSymbologyModel> Data { get; set; }
    }
}
