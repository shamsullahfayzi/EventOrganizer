using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using EventOrganizer.Models;
using EventOrganizer.Models.Entities;
using EventOrganizer.Models.Common;
using Microsoft.AspNetCore.Authorization;

namespace EventOrganizer.Pages.Admin.Bookings
{
    [Authorize(Roles = "Admin")]
    public class IndexModel : PageModel
    {
        private readonly ApplicationDbContext _context;

        public IndexModel(ApplicationDbContext context)
        {
            _context = context;
        }

        public IList<UserEvent> Registrations { get; set; }
        public IList<Event> Events { get; set; }

        public async Task OnGetAsync()
        {
            Registrations = await _context.UserEvents
                .Include(ue => ue.Event)
                .Include(ue => ue.User)
                .OrderByDescending(ue => ue.RegisteredDate)
                .ToListAsync();

            Events = await _context.Events
                .OrderBy(e => e.Title)
                .ToListAsync();
        }

      
    }
}