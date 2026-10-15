namespace RaceDay.API.Models
{
    public class EventSponsor
    {
        public int EventSponsorID { get; set; }
        public string? SponsorLevel { get; set; } // Nullable: Platinum/Gold/Silver/Bronze

        // Foreign Keys
        public int EventID { get; set; }
        public int SponsorID { get; set; }

        // Navigation Properties
        public Event? Event { get; set; }
        public Sponsor? Sponsor { get; set; }
    }
}