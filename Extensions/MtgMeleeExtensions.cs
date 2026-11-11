using Coaction.KickAssCardBot.Models.MtgMelee;
using Discord;

namespace Coaction.KickAssCardBot.Extensions
{
    public static class MtgMeleeExtensions
    {
        public static List<SelectMenuOptionBuilder> BuildRoundSelectionMenu(this Dictionary<string, TournamentResponse> rounds)
        {
            var optionBuilder = new List<SelectMenuOptionBuilder>();
            foreach (var round in rounds) {
                var firstDecklist = round.Value.Data.FirstOrDefault();
                if (firstDecklist == null) 
                {
                    continue;
                }

                optionBuilder.Add(new SelectMenuOptionBuilder(round.Key, $"mtgMeleeTournament-{firstDecklist.TournamentId}-{firstDecklist.RoundId}")); 
            }

            return optionBuilder;
        }
    }
}
