using Microsoft.AspNetCore.Identity;
using System;
using System.Collections.Generic;

namespace RaceDay.API.Models
{
    // Inherits from IdentityUser for built-in login/password features
    public class User : IdentityUser
    {
        public string FullName { get; set; } = string.Empty;
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public bool IsActive { get; set; } = true;
 
        public ICollection<Event> OrganisedEvents { get; set; } = new List<Event>();
        public ICollection<EventEnrollment> Enrolments { get; set; } = new List<EventEnrollment>();
    }
}
