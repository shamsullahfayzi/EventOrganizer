using NuGet.Protocol.Core.Types;

namespace EventOrganizer.Models.Entities
{
    public class Event
    {
        public int Id { get; set; }
        public string Title { get; set; }
        public string Description { get; set; }
        public string Location { get; set; }
        public int MaxAttendees { get; set; }
        public int CurrentAttendees { get; set; }
        public string? ThumbnailImage { get; set; }
        public DateTime EventDate { get; set; }
        public int CategoryId { get; set; }
        public Category Category { get; set; }
        public List<EventImage> Images { get; set; } = new List<EventImage>();
        public List<UserEvent> Attendees { get; set; } = new List<UserEvent>();
    }
}
