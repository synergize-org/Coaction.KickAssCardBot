using Coaction.KickAssCardBot.Models.MagicEventLocator;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;

namespace Coaction.KickAssCardBot.Manager
{
    public class WizardsEventLocatorManager
    {
        private readonly ILogger<WizardsEventLocatorManager> _logger;
        private readonly GeoLocationApiManager _geoLocationApiManager;
        private readonly IMemoryCache _memoryCache;

        public WizardsEventLocatorManager(ILogger<WizardsEventLocatorManager> logger, GeoLocationApiManager geoLocationApiManager, IMemoryCache memoryCache)
        {
            _logger = logger;
            _geoLocationApiManager = geoLocationApiManager;
            _memoryCache = memoryCache;
        }

        public async Task<List<Event>> GetEventLocationByZipcode(string zipcode, int miles = 25)
        {
            try
            { 
                var filteredEvents = new List<Event>();
                var geoLocationResults = await _geoLocationApiManager.GetLocationByZipCode(zipcode);
                var geoLocation = geoLocationResults.Results.FirstOrDefault();
                var meters = miles * 1609.344;
                using var client = new HttpClient
                {
                    BaseAddress = new Uri("https://api.tabletop.wizards.com/event-reservations-service/events/")
                };

                var regionalQualifierResults = await client.GetAsync(
                    $"search?" +
                    $"lat={geoLocation.Geometry.Location.Latitude}&" +
                    $"lng={geoLocation.Geometry.Location.Longitude}&" +
                    $"isPremium=false&" +
                    $"tag=magic:_the_gathering&" +
                    $"maxMeters={meters}&" +
                    $"pageSize=1000&" +
                    $"page=0&" +
                    $"sort=date&sortDirection=asc");

                regionalQualifierResults.EnsureSuccessStatusCode();
                var stringRegionalQualifierResults = await regionalQualifierResults.Content.ReadAsStringAsync();
                var regionalQualifierEvents =
                    JsonConvert.DeserializeObject<EventResults>(stringRegionalQualifierResults)?.Results;
                if (regionalQualifierEvents is { Count: > 0 })
                {
                    filteredEvents.AddRange(regionalQualifierEvents.Where(x =>
                        x.Tags.Contains("regional_championship") ||
                        x.Tags.Contains("regional_championship_qualifier") || x.Tags.Contains("store_championship")));

                    foreach (var magicEvent in filteredEvents)
                    {
                        await _memoryCache.GetOrCreateAsync($"magicEvent-{magicEvent.EventId}", entry =>
                        {
                            entry.SlidingExpiration = TimeSpan.FromDays(1);
                            return Task.FromResult(magicEvent);
                        });
                    }
                }

                return filteredEvents;

            }
            catch (Exception e)
            {
                var message = $"Failed to get Magic Events from zip code {zipcode} and mile range {miles} due to {e.Message}";
                _logger.LogError(e, message);
                throw new Exception(message, e);
            }
        }

        public async Task<Store> GetStoreDetails(uint storeId)
        {
            try
            {
                using var client = new HttpClient
                {
                    BaseAddress = new Uri("https://api.tabletop.wizards.com/event-reservations-service/Organizations/")
                };

                var storeDetails = await client.GetAsync(
                    $"by-ids?ids={storeId}");

                storeDetails.EnsureSuccessStatusCode();
                var storeDetailsResultString = await storeDetails.Content.ReadAsStringAsync();

                return JsonConvert.DeserializeObject<List<Store>>(storeDetailsResultString)?.FirstOrDefault();
            }
            catch (Exception e)
            {
                _logger.LogError(e, $"Failed to get store details for {storeId} due to {e.Message}");
                throw;
            }
        }

        public async Task<Event> GetMagicEvent(string eventId)
        {
            try
            {
                uint.TryParse(eventId, out var parsedEventId);
                var cacheKey = $"magicEvent-{parsedEventId}";

                if (_memoryCache.TryGetValue(cacheKey, out var cacheValue))
                {
                    if (cacheValue is Event cacheEvent)
                    {
                        return cacheEvent;
                    }
                }

                using var client = new HttpClient
                {
                    BaseAddress = new Uri("https://api.tabletop.wizards.com/event-reservations-service/events/")
                };

                var eventResponse = await client.GetAsync($"{eventId}");

                eventResponse.EnsureSuccessStatusCode();
                var stringEventResponseResults = await eventResponse.Content.ReadAsStringAsync();
                var magicEvent = JsonConvert.DeserializeObject<Event>(stringEventResponseResults) ?? throw new Exception($"Failed to deserialize magic event {eventId}.");
                magicEvent.Store = await GetStoreDetails(magicEvent.OrganizationId);

                _logger.LogDebug($"Successfully acquired magic event: {magicEvent.Name}.");
                _memoryCache.Set(cacheKey, magicEvent, TimeSpan.FromHours(3));
                return magicEvent;
            }
            catch (Exception e)
            {
                _logger.LogError(e, $"Failed to get event {eventId} due to {e.Message}.");
                throw;
            }
        }
    }
}
