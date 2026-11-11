using Newtonsoft.Json;

namespace Coaction.KickAssCardBot.Models.MagicEventLocator
{
    public class Event
    {
        [JsonProperty("eventId")]
        public uint EventId { get; set; }

        [JsonProperty("organizationId")]
        public uint OrganizationId { get; set; }

        [JsonProperty("groupId")]
        public string GroupId { get; set; }

        [JsonProperty("status")]
        public string Status { get; set; }

        [JsonProperty("name")]
        public string Name { get; set; }

        [JsonProperty("cost")]
        public double Cost { get; set; }

        [JsonProperty("currency")]
        public string Currency { get; set; }

        [JsonProperty("startingTableNumber")]
        public int StartingTableNumber { get; set; }

        [JsonProperty("hasTop8")]
        public bool HasTop8 { get; set; }

        [JsonProperty("isAdHoc")]
        public bool IsAdHoc { get; set; }

        [JsonProperty("isOnline")]
        public bool IsOnline { get; set; }

        [JsonProperty("officialEventTemplate")]
        public string OfficialEventTemplate { get; set; }

        [JsonProperty("reservations")]
        public int Reservations { get; set; }

        [JsonProperty("registrations")]
        public int Registrations { get; set; }

        [JsonProperty("isReserved")]
        public bool IsReserved { get; set; }

        [JsonProperty("isPrivate")]
        public bool IsPrivate { get; set; }

        [JsonProperty("description")]
        public string Description { get; set; }

        [JsonProperty("latitude")]
        public double Latitude { get; set; }

        [JsonProperty("longitude")]
        public double Longitude { get; set; }

        [JsonProperty("startDatetime")]
        public DateTime StartDatetime { get; set; }

        [JsonProperty("tags")]
        public List<string> Tags { get; set; }

        [JsonProperty("formatId")]
        public string FormatId { get; set; }

        [JsonProperty("distance")]
        public double Distance { get; set; }

        [JsonProperty("format")]
        public string Format { get; set; }

        [JsonProperty("requiredTeamSize")]
        public int RequiredTeamSize { get; set; }

        [JsonProperty("capacity")]
        public int? Capacity { get; set; }

        [JsonProperty("cardSetId")]
        public string CardSetId { get; set; }

        public Store Store { get; set; }
    }
}
