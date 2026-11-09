using System.Text.Json.Serialization;

namespace RaceDay.MVC.Models
{
    public class EventViewModel
    {
        // This tells the app to map "eventID" from the API to this property
        [JsonPropertyName("eventID")]
        public int EventID { get; set; }

        [JsonPropertyName("name")]
        public string Name { get; set; } = string.Empty;

        [JsonPropertyName("description")]
        public string Description { get; set; } = string.Empty;

        [JsonPropertyName("eventDate")]
        public DateTime EventDate { get; set; }

        [JsonPropertyName("location")]
        public string Location { get; set; } = string.Empty;

        [JsonPropertyName("maxParticipants")]
        public int MaxParticipants { get; set; }

        [JsonPropertyName("status")]
        public string Status { get; set; } = string.Empty;

        [JsonPropertyName("categoryID")]
        public int CategoryID { get; set; }

        [JsonPropertyName("imageUrl")]
        public string? ImageUrl { get; set; }

        public int TotalEnrolments { get; set; } = 0;
    }
}