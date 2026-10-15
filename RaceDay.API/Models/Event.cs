using System;
using System.Collections.Generic;

namespace RaceDay.API.Models
{
    public class Event
    {
        public int EventID { get; set; }
        public string Name { get; set; } = string.Empty;
        public string? Description { get; set; } // Nullable
        public DateTime EventDate { get; set; }
        public string Location { get; set; } = string.Empty;
        public int MaxParticipants { get; set; }
        public string Status { get; set; } = "Upcoming"; // Upcoming/Ongoing/Completed/Cancelled
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        // Foreign Keys
        public int CategoryID { get; set; }
        public string OrganiserID { get; set; } = string.Empty; // String because Identity uses string IDs

        // Navigation Properties
        public Category? Category { get; set; }
        public User? Organiser { get; set; }
        public ICollection<EventEnrollment> Enrolments { get; set; } = new List<EventEnrollment>();
        public ICollection<EventSponsor> EventSponsors { get; set; } = new List<EventSponsor>();
    }
}