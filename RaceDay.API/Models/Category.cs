using System;
using System.Collections.Generic;

namespace RaceDay.API.Models
{
    public class Category
    {
        public int CategoryID { get; set; }
        public string Name { get; set; } = string.Empty;
        public string? Description { get; set; } // Nullable
        public string AgeGroup { get; set; } = string.Empty;
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

      
        public ICollection<Event> Events { get; set; } = new List<Event>();
    }
}
