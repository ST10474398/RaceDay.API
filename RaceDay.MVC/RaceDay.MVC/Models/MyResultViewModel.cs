namespace RaceDay.MVC.Models
{
    public class MyResultViewModel
    {
        public string EventName { get; set; } = string.Empty;
        public DateTime EventDate { get; set; }
        public string FinishTime { get; set; } = string.Empty;
        public int Position { get; set; }
        public bool IsVerified { get; set; }
    }
}