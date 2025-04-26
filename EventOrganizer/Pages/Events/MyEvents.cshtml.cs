using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using EventOrganizer.Models;
using EventOrganizer.Models.Entities;
using Microsoft.AspNetCore.Identity;
using EventOrganizer.Models.Common;
using Microsoft.AspNetCore.Authorization;

namespace EventOrganizer.Pages.Events
{
    [Authorize(Roles = "User")]
    public class MyEventsModel : PageModel
    {
        private readonly ApplicationDbContext _context;
        private readonly UserManager<IdentityUser> _userManager;

        public MyEventsModel(ApplicationDbContext context, UserManager<IdentityUser> userManager)
        {
            _context = context;
            _userManager = userManager;
        }

        public IList<UserEvent> Registrations { get; set; }

        public async Task OnGetAsync()
        {
            var user = await _userManager.GetUserAsync(User);
            if (user != null)
            {
                Registrations = await _context.UserEvents
                    .Include(ue => ue.Event)
                    .ThenInclude(e => e.Category)
                    .Where(ue => ue.UserId == user.Id)
                    .OrderByDescending(ue => ue.Event.EventDate)
                    .ToListAsync();
            }
        }
    }
}