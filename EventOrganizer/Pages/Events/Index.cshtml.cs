using EventOrganizer.Models.Common;
using EventOrganizer.Models.Entities;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
namespace EventOrganizer.Pages.Events;
public class IndexModel : PageModel
{
    private readonly ApplicationDbContext _context;

    public IndexModel(ApplicationDbContext context)
    {
        _context = context;
    }

    public class EventViewModel
    {
        public int Id { get; set; }
        public string Title { get; set; }
        public string Description { get; set; }
        public string Location { get; set; }
        public DateTime EventDate { get; set; }
        public string CategoryName { get; set; }
        public string ThumbnailImage { get; set; }
        public int CurrentAttendees { get; set; }
        public int MaxAttendees { get; set; }
    }

    public PaginatedList<EventViewModel> Events { get; set; }
    public string CurrentFilter { get; set; }
    public string CurrentCategory { get; set; }
    public List<Category> Categories { get; set; }

    public async Task OnGetAsync(string searchString, string category, int? pageIndex)
    {
        CurrentFilter = searchString;
        CurrentCategory = category;

        var eventsQuery = _context.Events
            .Include(e => e.Category)
            .Select(e => new EventViewModel
            {
                Id = e.Id,
                Title = e.Title,
                Description = e.Description,
                Location = e.Location,
                EventDate = e.EventDate,
                CategoryName = e.Category.Name,
                ThumbnailImage = e.ThumbnailImage,
                CurrentAttendees = e.CurrentAttendees,
                MaxAttendees = e.MaxAttendees
            });

        if (!string.IsNullOrEmpty(searchString))
        {
            eventsQuery = eventsQuery.Where(e =>
                e.Title.Contains(searchString) ||
                e.Description.Contains(searchString) ||
                e.Location.Contains(searchString));
        }

        if (!string.IsNullOrEmpty(category))
        {
            eventsQuery = eventsQuery.Where(e => e.CategoryName == category);
        }

        Categories = await _context.Categories.ToListAsync();
        Events = await PaginatedList<EventViewModel>.CreateAsync(
            eventsQuery.AsNoTracking().OrderByDescending(e => e.EventDate),
            pageIndex ?? 1,
            9);
    }
}