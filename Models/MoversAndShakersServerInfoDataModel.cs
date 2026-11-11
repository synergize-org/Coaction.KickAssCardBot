using Newtonsoft.Json;

namespace Coaction.KickAssCardBot.Models
{
    public class MoversAndShakersServerInfoDataModel
    {
        [JsonProperty("ConfiguredDiscordGuilds")]
        public List<ulong> ListOfRegisteredDiscordGuilds { get; set; }
        public DateTime LastSuccessfulScrape { get; set; }
    }
}
