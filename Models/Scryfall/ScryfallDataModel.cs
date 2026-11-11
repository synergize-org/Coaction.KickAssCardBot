using Coaction.KickAssCardBot.Helpers.Scryfall;
using Newtonsoft.Json;

namespace Coaction.KickAssCardBot.Models.Scryfall
{
    public class ScryfallDataModel
    {
        public class CardData : IEquatable<CardData>
        {
            [JsonProperty("card_faces")]
            public List<CardFace> CardFaces { get; set; }

            [JsonProperty("object")]
            public string Object { get; set; }

            [JsonProperty("id")]
            public Guid Id { get; set; }

            [JsonProperty("oracle_id")]
            public Guid OracleId { get; set; }

            [JsonProperty("multiverse_ids")]
            public long[] MultiverseIds { get; set; }

            [JsonProperty("mtgo_id")]
            public long MtgoId { get; set; }

            [JsonProperty("mtgo_foil_id")]
            public long MtgoFoilId { get; set; }

            [JsonProperty("tcgplayer_id")]
            public long TcgplayerId { get; set; }

            [JsonProperty("name")]
            public string Name { get; set; }

            [JsonProperty("lang")]
            public string Lang { get; set; }

            [JsonProperty("released_at")]
            public DateTimeOffset ReleasedAt { get; set; }

            [JsonProperty("uri")]
            public string Uri { get; set; }

            [JsonProperty("scryfall_uri")]
            public string ScryfallUri { get; set; }

            [JsonProperty("layout")]
            public string Layout { get; set; }

            [JsonProperty("highres_image")]
            public bool HighresImage { get; set; }

            [JsonProperty("image_uris")]
            public ImageUris ImageUris { get; set; }

            [JsonProperty("mana_cost")]
            public string ManaCost { get; set; }

            [JsonProperty("cmc")]
            public long Cmc { get; set; }

            [JsonProperty("type_line")]
            public string TypeLine { get; set; }

            [JsonProperty("power")]
            public string Power { get; set; }

            [JsonProperty("toughness")]
            public string Toughness { get; set; }

            [JsonProperty("oracle_text")]
            public string OracleText { get; set; }

            [JsonProperty("colors")]
            public string[] Colors { get; set; }

            [JsonProperty("color_identity")]
            public string[] ColorIdentity { get; set; }

            [JsonProperty("legalities")]
            public Legalities Legalities { get; set; }

            [JsonProperty("loyalty")]
            public uint Loyalty { get; set; }

            public Dictionary<string, string> AllLegalities => new()
            {
                {"Standard ", LegalityDictionary.Legality[Legalities.Standard]},
                {"Modern   ", LegalityDictionary.Legality[Legalities.Modern]},
                {"Legacy   ", LegalityDictionary.Legality[Legalities.Legacy]},
                {"Vintage  ", LegalityDictionary.Legality[Legalities.Vintage]},
                {"Pauper   ", LegalityDictionary.Legality[Legalities.Pauper]},
                {"Pioneer  ", LegalityDictionary.Legality[Legalities.Pioneer]},
                {"Commander", LegalityDictionary.Legality[Legalities.Commander]},
                {"Historic ", LegalityDictionary.Legality[Legalities.Historic]}
            };

            [JsonProperty("games")]
            public string[] Games { get; set; }

            [JsonProperty("reserved")]
            public bool Reserved { get; set; }

            [JsonProperty("foil")]
            public bool Foil { get; set; }

            [JsonProperty("nonfoil")]
            public bool Nonfoil { get; set; }

            [JsonProperty("oversized")]
            public bool Oversized { get; set; }

            [JsonProperty("promo")]
            public bool Promo { get; set; }

            [JsonProperty("reprint")]
            public bool Reprint { get; set; }

            [JsonProperty("set")]
            public string Set { get; set; }

            [JsonProperty("set_name")]
            public string SetName { get; set; }

            [JsonProperty("set_uri")]
            public string SetUri { get; set; }

            [JsonProperty("set_search_uri")]
            public string SetSearchUri { get; set; }

            [JsonProperty("scryfall_set_uri")]
            public string ScryfallSetUri { get; set; }

            [JsonProperty("rulings_uri")]
            public string RulingsUri { get; set; }

            [JsonProperty("prints_search_uri")]
            public string PrintsSearchUri { get; set; }

            [JsonProperty("collector_number")]
            public string CollectorNumber { get; set; }

            [JsonProperty("digital")]
            public bool Digital { get; set; }

            [JsonProperty("rarity")]
            public string Rarity { get; set; }

            [JsonProperty("flavor_text")]
            public string FlavorText { get; set; }

            [JsonProperty("illustration_id")]
            public Guid IllustrationId { get; set; }

            [JsonProperty("artist")]
            public string Artist { get; set; }

            [JsonProperty("border_color")]
            public string BorderColor { get; set; }

            [JsonProperty("frame")]
            public string Frame { get; set; }

            [JsonProperty("full_art")]
            public bool FullArt { get; set; }

            [JsonProperty("story_spotlight")]
            public bool StorySpotlight { get; set; }

            [JsonProperty("edhrec_rank")]
            public long EdhrecRank { get; set; }

            [JsonProperty("prices")]
            public Prices Prices { get; set; }

            [JsonProperty("related_uris")]
            public RelatedUris RelatedUris { get; set; }

            [JsonProperty("purchase_uris")]
            public PurchaseUris PurchaseUris { get; set; }

            public bool Equals(CardData? other)
            {
                return other != null && Name == other.Name && Set == other.Set;
            }

            public override int GetHashCode()
            {
                var hashName = Name == null ? 0 : Name.GetHashCode();
                var hashId = Set == null ? 0 : Set.GetHashCode();

                return hashName ^ hashId;
            }
        }

        public class ImageUris
        {
            [JsonProperty("small")]
            public string Small { get; set; }

            [JsonProperty("normal")]
            public string Normal { get; set; }

            [JsonProperty("large")]
            public string Large { get; set; }

            [JsonProperty("png")]
            public string Png { get; set; }

            [JsonProperty("art_crop")]
            public string ArtCrop { get; set; }

            [JsonProperty("border_crop")]
            public string BorderCrop { get; set; }
        }

        public class Legalities
        {
            [JsonProperty("standard")]
            public string Standard { get; set; }

            [JsonProperty("future")]
            public string Future { get; set; }

            [JsonProperty("frontier")]
            public string Frontier { get; set; }

            [JsonProperty("modern")]
            public string Modern { get; set; }

            [JsonProperty("legacy")]
            public string Legacy { get; set; }

            [JsonProperty("pauper")]
            public string Pauper { get; set; }

            [JsonProperty("vintage")]
            public string Vintage { get; set; }

            [JsonProperty("penny")]
            public string Penny { get; set; }

            [JsonProperty("commander")]
            public string Commander { get; set; }

            [JsonProperty("duel")]
            public string Duel { get; set; }

            [JsonProperty("oldschool")]
            public string Oldschool { get; set; }

            [JsonProperty("pioneer")]
            public string Pioneer { get; set; }
            [JsonProperty("historic")]
            public string Historic { get; set; }
        }

        public class Prices
        {
            [JsonProperty("usd")]
            public string Usd { get; set; }

            [JsonProperty("usd_foil")]
            public string UsdFoil { get; set; }

            [JsonProperty("eur")]
            public string Eur { get; set; }

            [JsonProperty("tix")]
            public string Tix { get; set; }
        }

        public class PurchaseUris
        {
            [JsonProperty("tcgplayer")]
            public string Tcgplayer { get; set; }

            [JsonProperty("cardmarket")]
            public string Cardmarket { get; set; }

            [JsonProperty("cardhoarder")]
            public string Cardhoarder { get; set; }
        }

        public class RelatedUris
        {
            [JsonProperty("gatherer")]
            public string Gatherer { get; set; }

            [JsonProperty("tcgplayer_decks")]
            public string TcgplayerDecks { get; set; }

            [JsonProperty("edhrec")]
            public string Edhrec { get; set; }

            [JsonProperty("mtgtop8")]
            public string Mtgtop8 { get; set; }
        }

        public class CardFace
        {
            public string @object { get; set; }
            public string name { get; set; }
            public string mana_cost { get; set; }
            public string type_line { get; set; }
            public string oracle_text { get; set; }
            public List<string> colors { get; set; }
            public string power { get; set; }
            public string toughness { get; set; }
            public string artist { get; set; }
            public string artist_id { get; set; }
            public string illustration_id { get; set; }
            public ImageUris image_uris { get; set; }
            public List<string> color_indicator { get; set; }
            public string flavor_text { get; set; }
            [JsonProperty("defense")]
            public uint Defense { get; set; }
        }
    }
}
