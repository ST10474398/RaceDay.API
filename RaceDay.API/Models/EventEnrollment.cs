using System;

namespace RaceDay.API.Models
{
    public class EventEnrollment
    {
        public int EnrolmentID { get; set; }
        public string Status { get; set; } = "Pending"; // Pending/Approved/Rejected/Cancelled
        public DateTime EnrolledAt { get; set; } = DateTime.UtcNow;
        public DateTime? UpdatedAt { get; set; } // Nullable

        // Foreign Keys
        public string UserID { get; set; } = string.Empty;
        public int EventID { get; set; }

        // Navigation Properties
        public User? User { get; set; }
        public Event? Event { get; set; }
        public Result? Result { get; set; }
    }
}