using Coaction.KickAssCardBot.Models.MtgMelee;
using Discord;

namespace Coaction.KickAssCardBot.Embed_Output
{
    public static class MtgMeleeOutput
    {
        public static EmbedBuilder OutputDecklistAggregator(Dictionary<string, TournamentResponse> tournamentResults, string roundId = "")
        {
            TournamentResponse response = null;
            string roundName = null;
            if (!string.IsNullOrEmpty(roundId))
            {
                if (long.TryParse(roundId, out var parsedRoundId))
                {
                    var specificRound = tournamentResults.FirstOrDefault(x => x.Value.Data.Any(x => x.RoundId == parsedRoundId));
                    response = specificRound.Value;
                    roundName = specificRound.Key;
                }
            }

            var targetRound = response ?? tournamentResults.FirstOrDefault().Value;
            var embedBuilder = new EmbedBuilder();
            embedBuilder.WithFooter(targetRound.Status);
            embedBuilder.WithTimestamp(targetRound.StartDate);
            embedBuilder.Url = $"https://melee.gg/Tournament/View/{targetRound.Data.FirstOrDefault()?.TournamentId}";
            embedBuilder.Color = Color.DarkOrange;
            embedBuilder.Title = $"{targetRound.TournamentName} - {roundName ?? tournamentResults.FirstOrDefault().Key}";
            embedBuilder.Description = $"Decklist aggregator. Collects all decklists by name and groups them. Capped at 25 different deck lists due to discord limitation. Sorts by most number of decks to get best data possible.";
            var filteredByDecklist = targetRound.Data.Where(x => x.Decklists.Any()).GroupBy(x => x.Decklists?.FirstOrDefault()?.DecklistName).OrderByDescending(x => x.Count()).Take(25);
            foreach (var deck in filteredByDecklist)
            {
                embedBuilder.AddField(deck.Key, deck.Count(), true);
            }

            return embedBuilder;
        }

        public static EmbedBuilder OutputTop16Decklists(Dictionary<string, TournamentResponse> tournamentResults)
        {
            var embedBuilder = new EmbedBuilder();
            var decks = tournamentResults.LastOrDefault().Value.Data.Take(16);
            var firstRound = decks.FirstOrDefault();
            embedBuilder.WithFooter(tournamentResults.FirstOrDefault().Value.Status);
            embedBuilder.WithTimestamp(tournamentResults.FirstOrDefault().Value.StartDate);
            embedBuilder.Url = $"https://melee.gg/Tournament/View/{firstRound?.TournamentId}";
            embedBuilder.Title = $"{tournamentResults.FirstOrDefault().Value.TournamentName}";
            embedBuilder.Description = "Attempts to grab the top 16 decks from the latest round of the tournament.";
            embedBuilder.Color = Color.DarkOrange;
            foreach (var deck in decks)
            {
                var displayName = deck.Team.Name ?? deck.Team.Players.FirstOrDefault()?.DisplayName;
                var decklist = deck.Decklists.FirstOrDefault();
                string fieldValue = "No Deck Data";
                if (decklist != null)
                {
                    fieldValue = $"[{decklist?.DecklistName}](https://melee.gg/Decklist/View/{decklist?.DecklistId})";
                }
                embedBuilder.AddField($"{displayName} ({deck.MatchWins}-{deck.MatchLosses}-{deck.MatchDraws})", fieldValue, true);
            }

            return embedBuilder;
        }
    }
}
