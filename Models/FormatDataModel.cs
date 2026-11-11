namespace Coaction.KickAssCardBot.Models
{
    public class FormatDataModel
    {
        public List<MoversShakersRabbitModel.DailyWinner> DailyWinners { get; set; }
        public List<MoversShakersRabbitModel.DailyLoser> DailyLosers { get; set; }
        public List<MoversShakersRabbitModel.WeeklyWinner> WeeklyWinners { get; set; }
        public List<MoversShakersRabbitModel.WeeklyLoser> WeeklyLosers { get; set; }

        public FormatDataModel(MoversShakersRabbitModel.Standard cardData)
        {
            DailyWinners = cardData.DailyWinners;
            DailyLosers = cardData.DailyLosers;
            WeeklyWinners = cardData.WeeklyWinners;
            WeeklyLosers = cardData.WeeklyLosers;
        }

        public FormatDataModel(MoversShakersRabbitModel.Modern cardData)
        {
            DailyWinners = cardData.DailyWinners;
            DailyLosers = cardData.DailyLosers;
            WeeklyWinners = cardData.WeeklyWinners;
            WeeklyLosers = cardData.WeeklyLosers;
        }

        public FormatDataModel(MoversShakersRabbitModel.Pioneer cardData)
        {
            DailyWinners = cardData.DailyWinners;
            DailyLosers = cardData.DailyLosers;
            WeeklyWinners = cardData.WeeklyWinners;
            WeeklyLosers = cardData.WeeklyLosers;
        }

        public FormatDataModel(MoversShakersRabbitModel.Pauper cardData)
        {
            DailyWinners = cardData.DailyWinners;
            DailyLosers = cardData.DailyLosers;
            WeeklyWinners = cardData.WeeklyWinners;
            WeeklyLosers = cardData.WeeklyLosers;
        }

        public FormatDataModel(MoversShakersRabbitModel.Legacy cardData)
        {
            DailyWinners = cardData.DailyWinners;
            DailyLosers = cardData.DailyLosers;
            WeeklyWinners = cardData.WeeklyWinners;
            WeeklyLosers = cardData.WeeklyLosers;
        }

        public FormatDataModel(MoversShakersRabbitModel.Vintage cardData)
        {
            DailyWinners = cardData.DailyWinners;
            DailyLosers = cardData.DailyLosers;
            WeeklyWinners = cardData.WeeklyWinners;
            WeeklyLosers = cardData.WeeklyLosers;
        }
    }
}
