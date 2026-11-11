using Newtonsoft.Json;

namespace Coaction.KickAssCardBot.Models.Scryfall
{
    public class ScryFallCardRulingsModel
    {
        public class Rule : IComparable<Rule>
        {
            public string @object { get; set; }
            public Guid oracle_id { get; set; }
            public string source { get; set; }
            public string published_at { get; set; }
            public string comment { get; set; }
            public int CompareTo(Rule other)
            {
                if (@object == other.@object && oracle_id == other.oracle_id && source == other.source &&
                    published_at == other.published_at && comment == other.comment)
                {
                    return 1;
                }

                return 0;
            }
        }
            public string @object { get; set; }
            public bool has_more { get; set; }
            [JsonProperty("data")]
            public List<Rule> Rules { get; set; }        
    }
}
