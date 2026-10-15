using System;
using System.Collections.Generic;

namespace RaceDay.API.Models
{
    public class Sponsor
    {
        public int SponsorID { get; set; }
        public string Name { get; set; } = string.Empty;
        public string? LogoUrl { get; set; } // Nullable
        public string? Website { get; set; } // Nullable
        public string? ContactEmail { get; set; } // Nullable
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        // Navigation Property
        public ICollection<EventSponsor> EventSponsors { get; set; } = new List<EventSponsor>();
    }
}