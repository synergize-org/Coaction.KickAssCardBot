namespace Coaction.KickAssCardBot.Models
{
    public class MoversShakersRabbitModel
    {
        public class DailyWinner
        {
            public string PriceChange { get; set; }
            public string Name { get; set; }
            public string TotalPrice { get; set; }
            public string ChangePercentage { get; set; }
        }

        public class DailyLoser
        {
            public string PriceChange { get; set; }
            public string Name { get; set; }
            public string TotalPrice { get; set; }
            public string ChangePercentage { get; set; }
        }

        public class WeeklyWinner
        {
            public string PriceChange { get; set; }
            public string Name { get; set; }
            public string TotalPrice { get; set; }
            public string ChangePercentage { get; set; }
        }

        public class WeeklyLoser
        {
            public string PriceChange { get; set; }
            public string Name { get; set; }
            public string TotalPrice { get; set; }
            public string ChangePercentage { get; set; }
        }

        public class Standard
        {
            public List<DailyWinner> DailyWinners { get; set; }
            public List<DailyLoser> DailyLosers { get; set; }
            public List<WeeklyWinner> WeeklyWinners { get; set; }
            public List<WeeklyLoser> WeeklyLosers { get; set; }
        }

        public class Modern
        {
            public List<DailyWinner> DailyWinners { get; set; }
            public List<DailyLoser> DailyLosers { get; set; }
            public List<WeeklyWinner> WeeklyWinners { get; set; }
            public List<WeeklyLoser> WeeklyLosers { get; set; }
        }

        public class Pioneer
        {
            public List<DailyWinner> DailyWinners { get; set; }
            public List<DailyLoser> DailyLosers { get; set; }
            public List<WeeklyWinner> WeeklyWinners { get; set; }
            public List<WeeklyLoser> WeeklyLosers { get; set; }
        }

        public class Pauper
        {
            public List<DailyWinner> DailyWinners { get; set; }
            public List<DailyLoser> DailyLosers { get; set; }
            public List<WeeklyWinner> WeeklyWinners { get; set; }
            public List<WeeklyLoser> WeeklyLosers { get; set; }
        }

        public class Legacy
        {
            public List<DailyWinner> DailyWinners { get; set; }
            public List<DailyLoser> DailyLosers { get; set; }
            public List<WeeklyWinner> WeeklyWinners { get; set; }
            public List<WeeklyLoser> WeeklyLosers { get; set; }
        }

        public class Vintage
        {
            public List<DailyWinner> DailyWinners { get; set; }
            public List<DailyLoser> DailyLosers { get; set; }
            public List<WeeklyWinner> WeeklyWinners { get; set; }
            public List<WeeklyLoser> WeeklyLosers { get; set; }
        }

        public class Root
        {
            public Standard Standard { get; set; }
            public Modern Modern { get; set; }
            public Pioneer Pioneer { get; set; }
            public Pauper Pauper { get; set; }
            public Legacy Legacy { get; set; }
            public Vintage Vintage { get; set; }
            public DateTime LastUpdatedAt { get; set; }
            public DateTime NextScrapeTime { get; set; }
        }
    }
}
