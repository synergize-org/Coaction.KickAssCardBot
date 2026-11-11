namespace Coaction.KickAssCardBot.Models.MtgMelee
{
    public class TournamentResponse
    {
        public string TournamentName { get; set; }
        public int PlayerCount { get; set; }
        public string Status { get; set; }
        public DateTime StartDate { get; set; }
        public long Draw { get; set; }
        public long RecordsTotal { get; set; }
        public long RecordsFiltered { get; set; }
        public List<DataItem> Data { get; set; }
        public string? Download { get; set; }
    }

    public class DataItem
    {
        public Team Team { get; set; }
        public DateTime DateCreated { get; set; }
        public long MatchingMethod { get; set; }
        public bool StandingsPublished { get; set; }
        public long FinalTiebreaker { get; set; }
        public long OpponentGameWinPercentage { get; set; }
        public long OpponentMatchWinPercentage { get; set; }
        public long TeamGameWinPercentage { get; set; }
        public long GameCount { get; set; }
        public long GameDraws { get; set; }
        public long GameLosses { get; set; }
        public long GameWins { get; set; }
        public long MatchCount { get; set; }
        public long MatchDraws { get; set; }
        public long MatchLosses { get; set; }
        public long MatchWins { get; set; }
        public long OpponentCount { get; set; }
        public long PhaseSortOrder { get; set; }
        public long Points { get; set; }
        public long Rank { get; set; }
        public long RoundNumber { get; set; }
        public long ID { get; set; }
        public long PhaseId { get; set; }
        public long RoundId { get; set; }
        public long TeamId { get; set; }
        public long TournamentId { get; set; }
        public string Format { get; set; }
        public string PhaseName { get; set; }
        public string MatchingMethodDescription { get; set; }
        public string Round { get; set; }
        public DecklistsItem[] Decklists { get; set; }
        public string GameRecord { get; set; }
        public string MatchRecord { get; set; }
        public string FormatDescription { get; set; }
    }

    public class Team
    {
        public PlayersItem[] Players { get; set; }
        public long ID { get; set; }
        public string? Name { get; set; }
        public string StatusDescription { get; set; }
        public bool IsActive { get; set; }
    }

    public class PlayersItem
    {
        public long TeamId { get; set; }
        public long ID { get; set; }
        public string ScreenName { get; set; }
        public MetadataDictionary MetadataDictionary { get; set; }
        public long ProfileImageVersion { get; set; }
        public string DisplayName { get; set; }
        public string DisplayNameLastFirst { get; set; }
        public string Username { get; set; }
        public string ArenaScreenName { get; set; }
        public long DciNumber { get; set; }
        public string DiscordUsername { get; set; }
        public string FirstName { get; set; }
        public string? GemPlayerId { get; set; }
        public string LastName { get; set; }
        public string MtgoScreenName { get; set; }
        public string Name { get; set; }
        public string NameLastFirst { get; set; }
        public string? AsmoConnectId { get; set; }
        public string LanguageDescription { get; set; }
        public string PronounsDescription { get; set; }
    }

    public class MetadataDictionary
    {
    }

    public class DecklistsItem
    {
        public long DecklistId { get; set; }
        public long PlayerId { get; set; }
        public string DecklistName { get; set; }
        public string Format { get; set; }
    }
}
