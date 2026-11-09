namespace RaceDay.MVC.Models
{
    public class EnrolmentViewModel
    {
        public int EnrolmentID { get; set; }
        public string ParticipantName { get; set; } = string.Empty;
        public string ParticipantEmail { get; set; } = string.Empty;
        public string Status { get; set; } = string.Empty;
        public DateTime EnrolledAt { get; set; }
    }
}