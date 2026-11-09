namespace RaceDay.MVC.Models
{
    public class MyEnrolmentViewModel
    {
        public int EnrolmentID { get; set; }
        public string Status { get; set; } = string.Empty;
        public DateTime EnrolledAt { get; set; }

        // Event Details
        public int EventID { get; set; }
        public string EventName { get; set; } = string.Empty;
        public string EventLocation { get; set; } = string.Empty;
        public DateTime EventDate { get; set; }

        // Result Details (Optional)
        public string? FinishTime { get; set; }
        public int? Position { get; set; }
    }
}