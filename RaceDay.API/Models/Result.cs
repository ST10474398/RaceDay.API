using System;

namespace RaceDay.API.Models
{
    public class Result
    {
        public int ResultID { get; set; }
        public TimeSpan FinishTime { get; set; }
        public int Position { get; set; }
        public DateTime RecordedAt { get; set; } = DateTime.UtcNow;
        public bool IsVerified { get; set; } = false;

        // Foreign Key (Unique - one result per enrolment)
        public int EnrolmentID { get; set; }

        // Navigation Property
        public EventEnrollment? Enrolment { get; set; }
    }
}