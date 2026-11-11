namespace Coaction.KickAssCardBot.Models.MtgMelee
{
    public class TournamentDetailsResponse
    {
        public long ID { get; set; }
        public string Name { get; set; }
        public string BrandImageSource { get; set; }
        public DateTime StartDate { get; set; }
        public string Status { get; set; }
        public long OrganizationId { get; set; }
        public string OrganizationName { get; set; }
        public string Format { get; set; }
        public long ParticipatorCount { get; set; }
    }
}
