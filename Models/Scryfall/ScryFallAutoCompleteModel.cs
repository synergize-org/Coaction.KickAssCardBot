namespace Coaction.KickAssCardBot.Models.Scryfall
{
    public class ScryFallAutoCompleteModel
    {
        public string @object { get; set; }
        public int total_values { get; set; }
        public List<string> data { get; set; }

    }
}
