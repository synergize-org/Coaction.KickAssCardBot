using Coaction.KickAssCardBot.Enums;
using Coaction.KickAssCardBot.Extensions;
using Newtonsoft.Json;

namespace Coaction.KickAssCardBot.Models.Scryfall
{
    public class ScryfallPrints
    {
        [JsonProperty("object")]
        public string Object { get; set; }

        [JsonProperty("total_cards")]
        public int TotalCards { get; set; }

        [JsonProperty("has_more")]
        public bool HasMore { get; set; }

        [JsonProperty("data")]
        public List<ScryfallDataModel.CardData> CardData { get; set; }

        public ScryfallPrints FilterOnlyPrintsWithPricingData()
        {
            CardData = this.CardData.Where(x => x.Prices?.Usd != null || x.Prices?.UsdFoil != null).ToList();
            return this;
        }

        public ScryfallPrints FilterByGameType(GameType gameType)
        {
            CardData = this.CardData.Where(x => x.Games.Contains(gameType.ToScryfallString())).ToList();
            return this;
        }

        public ScryfallPrints RemoveDuplicates()
        {
            CardData = CardData.DistinctBy(x => x.Name).ToList();
            return this;
        }
    }
}
