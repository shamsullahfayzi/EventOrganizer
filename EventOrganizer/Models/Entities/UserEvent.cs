using Microsoft.AspNetCore.Identity;

namespace EventOrganizer.Models.Entities
{
    public class UserEvent
    {
        public string UserId { get; set; }
        public IdentityUser User { get; set; }
        public int EventId { get; set; }
        public Event Event { get; set; }
        public DateTime RegisteredDate { get; set; }
    }
}
