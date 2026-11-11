using Coaction.KickAssCardBot.Factories;
using Coaction.KickAssCardBot.Models.MtgMelee;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;

namespace Coaction.KickAssCardBot.Manager
{
    public class MtgMeleeManager
    {
        private readonly ILogger<MtgMeleeManager> _logger;
        private readonly IConfiguration _configuration;
        private readonly HtmlAgilityManager _htmlAgilityManager;
        private readonly IMemoryCache _memoryCache;

        public MtgMeleeManager(ILogger<MtgMeleeManager> logger, IConfiguration configuration, HtmlAgilityManager htmAgilityManager, IMemoryCache memoryCache)
        {
            _logger = logger;
            _configuration = configuration;
            _htmlAgilityManager = htmAgilityManager;
            _memoryCache = memoryCache;
        }

        public async Task<Dictionary<string, TournamentResponse>> GetTournamentResults(string tournamentId)
        {
            string cacheKey = $"mtgMeleeTournament-{tournamentId}";
            var cacheValue = _memoryCache.Get(cacheKey);
            if (cacheValue is Dictionary<string, TournamentResponse> data)
            {
                return data;
            }
            var results = new Dictionary<string, TournamentResponse>();
            var roundIds = await _htmlAgilityManager.GetAllRoundIds(tournamentId);     
            var tournamentDetails = await GetTournamentDetails(tournamentId);
            foreach (var roundId in roundIds) 
            {
                var parsedResponse = await GetTournamentResponse(roundId.Value, 500);
                if (parsedResponse != null)
                {
                    if (parsedResponse.Data.Count == 0)
                    {
                        continue;
                    }

                    var maximumResults = (int)parsedResponse.RecordsTotal;
                    _logger.LogInformation($"Number of decks {maximumResults} for tournament {tournamentId} in {roundId.Key}.");
                    if (parsedResponse.Data.Count < maximumResults)
                    {
                        _logger.LogInformation($"Unable to obtain all decks due to number. Attempting to obtain additional decks.");
                        while (parsedResponse.Data.Count != maximumResults)
                        {
                            _logger.LogInformation($"Current deck count {parsedResponse.Data.Count} out of {maximumResults}.");
                            var additionalDecks = await GetTournamentResponse(roundId.Value, maximumResults - parsedResponse.Data.Count, parsedResponse.Data.Count);
                            parsedResponse.Data.AddRange(additionalDecks.Data);
                        }

                        _logger.LogInformation($"{parsedResponse.Data.Count} out of {maximumResults} decks acquired.");
                    }

                    parsedResponse.TournamentName = tournamentDetails?.Name;
                    parsedResponse.PlayerCount = (int)tournamentDetails?.ParticipatorCount;
                    parsedResponse.Status = tournamentDetails.Status;
                    parsedResponse.StartDate = tournamentDetails.StartDate;
                    results[roundId.Key] = parsedResponse;
                }
                else {
                    _logger.LogError($"Failed to parse results for {roundId.Key} ({roundId.Value}) via tournament {tournamentId}.");
                }
            }

            _memoryCache.Set(cacheKey, results);
            return results;
        }

        private async Task<TournamentResponse> GetTournamentResponse(string roundId, int standingsLength, int standingsStart = 0)
        {
            using var httpClient = HttpClientFactory.GetValidMtgMeleeClient();
            using var formDataRequest = BuildRoundStandingsRequest(roundId, standingsLength, standingsStart);
            var request = await httpClient.PostAsync($"/Standing/GetRoundStandings", formDataRequest);
            var responseRaw = await request.Content.ReadAsStringAsync();
            var parsedResponse = JsonConvert.DeserializeObject<TournamentResponse>(responseRaw, new JsonSerializerSettings()
            {
                NullValueHandling = NullValueHandling.Ignore,
            });

            return parsedResponse;
        }

        private async Task<TournamentDetailsResponse> GetTournamentDetails(string tournamentId)
        {
            using var httpClient = HttpClientFactory.GetValidMtgMeleeClient();
            var request = await httpClient.GetAsync($"/Tournament/GetTournamentDetails?id={tournamentId}");
            var responseRaw = await request.Content.ReadAsStringAsync();
            var parsedResponse = JsonConvert.DeserializeObject<TournamentDetailsResponse>(responseRaw, new JsonSerializerSettings()
            {
                NullValueHandling = NullValueHandling.Ignore,
            });

            return parsedResponse;
        }

        private MultipartFormDataContent BuildRoundStandingsRequest(string roundId, int length, int start = 0)
        {
            string[] columnData = {
            "Player", "Decklists", "MatchRecord", "GameRecord", "Points",
            "OpponentMatchWinPercentage", "TeamGameWinPercentage", "OpponentGameWinPercentage",
            "FinalTiebreaker", "OpponentCount"
            };
            
            var formData = new MultipartFormDataContent
            {
                { new StringContent("1"), "draw" },
                { new StringContent("Rank"), "columns[0][data]" },
                { new StringContent("Rank"), "columns[0][name]" },
                { new StringContent("true"), "columns[0][searchable]" },
                { new StringContent("true"), "columns[0][orderable]" },
                { new StringContent(string.Empty), "columns[0][search][value]" },
                { new StringContent("false"), "columns[0][search][regex]" }
            };

            for (int i = 1; i <= 10; i++)
            {
                formData.Add(new StringContent(columnData[i - 1]), $"columns[{i}][data]");
                formData.Add(new StringContent(columnData[i - 1]), $"columns[{i}][name]");
                formData.Add(new StringContent(i == 5 || i == 10 ? "true" : "false"), $"columns[{i}][searchable]");
                formData.Add(new StringContent(i == 5 || i >= 6 ? "true" : "false"), $"columns[{i}][orderable]");
                formData.Add(new StringContent(string.Empty), $"columns[{i}][search][value]");
                formData.Add(new StringContent("false"), $"columns[{i}][search][regex]");
            }

            // Add ordering
            formData.Add(new StringContent("0"), "order[0][column]");
            formData.Add(new StringContent("asc"), "order[0][dir]");

            // Add pagination
            formData.Add(new StringContent(start.ToString()), "start");
            formData.Add(new StringContent(length.ToString()), "length");

            // Add search parameters
            formData.Add(new StringContent(string.Empty), "search[value]");
            formData.Add(new StringContent("false"), "search[regex]");

            // Add dynamic roundId
            formData.Add(new StringContent(roundId), "roundId");

            return formData;
        }
    }
}
