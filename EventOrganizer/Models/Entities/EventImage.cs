namespace EventOrganizer.Models.Entities
{
    public class EventImage
    {
        public int Id { get; set; }
        public string ImageUrl { get; set; }
        public string Caption { get; set; }
        public int EventId { get; set; }
        public Event Event { get; set; }
    }
}
