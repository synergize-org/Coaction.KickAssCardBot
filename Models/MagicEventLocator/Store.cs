using Newtonsoft.Json;

namespace Coaction.KickAssCardBot.Models.MagicEventLocator
{
    public class Store
    {
        [JsonProperty("acceptedTermsAndConditionsAt")]
        public DateTime AcceptedTermsAndConditionsAt { get; set; }

        [JsonProperty("id")]
        public int Id { get; set; }

        [JsonProperty("name")]
        public string Name { get; set; }

        [JsonProperty("latitude")]
        public double Latitude { get; set; }

        [JsonProperty("longitude")]
        public double Longitude { get; set; }

        [JsonProperty("phoneNumber")]
        public string PhoneNumber { get; set; }

        [JsonProperty("emailAddress")]
        public string EmailAddress { get; set; }

        [JsonProperty("showEmailInSEL")]
        public bool ShowEmailInSel { get; set; }

        [JsonProperty("website")]
        public string Website { get; set; }

        [JsonProperty("websites")]
        public List<object> Websites { get; set; }

        [JsonProperty("phoneNumbers")]
        public List<object> PhoneNumbers { get; set; }

        [JsonProperty("brands")]
        public List<object> Brands { get; set; }

        [JsonProperty("postalAddress")]
        public string PostalAddress { get; set; }

        [JsonProperty("address")]
        public string Address { get; set; }

        [JsonProperty("city")]
        public string City { get; set; }

        [JsonProperty("state")]
        public string State { get; set; }

        [JsonProperty("country")]
        public string Country { get; set; }

        [JsonProperty("postalCode")]
        public string PostalCode { get; set; }

        [JsonProperty("isPremium")]
        public bool IsPremium { get; set; }

        [JsonProperty("isTestStore")]
        public bool IsTestStore { get; set; }

        [JsonProperty("status")]
        public string Status { get; set; }

        [JsonProperty("language")]
        public string Language { get; set; }

        [JsonProperty("wpnRep")]
        public string WpnRep { get; set; }

        [JsonProperty("distributor")]
        public Distributor Distributor { get; set; }

        [JsonProperty("groups")]
        public List<string> Groups { get; set; }

        [JsonProperty("isActive")]
        public bool IsActive { get; set; }
    }
}
