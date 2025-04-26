using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using EventOrganizer.Models;
using EventOrganizer.Models.Common;
using EventOrganizer.Models.Entities;

namespace EventOrganizer.Pages.Admin.Events
{
    public class IndexModel : PageModel
    {
        private readonly ApplicationDbContext _context;

        public IndexModel(ApplicationDbContext context)
        {
            _context = context;
        }

        public IList<Event> Events { get; set; }

        public async Task OnGetAsync()
        {
            Events = await _context.Events.ToListAsync();
        }

        public async Task<IActionResult> OnPostDeleteAsync(int id)
        {
            var @event = await _context.Events.FindAsync(id);

            if (@event != null)
            {
                _context.Events.Remove(@event);
                await _context.SaveChangesAsync();
            }

            return RedirectToPage();
        }
    }
}