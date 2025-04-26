using EventOrganizer.Models.Common;
using EventOrganizer.Models.Entities;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
namespace EventOrganizer.Pages.Admin
{
    public class IndexModel : PageModel
    {
        private readonly ApplicationDbContext _context;
        private readonly UserManager<IdentityUser> _userManager;

        public IndexModel(ApplicationDbContext context, UserManager<IdentityUser> userManager)
        {
            _context = context;
            _userManager = userManager;
        }

        public class DashboardStats
        {
            public int TotalEvents { get; set; }
            public int TotalUsers { get; set; }
            public int TotalCategories { get; set; }
            public int TotalBookings { get; set; }
        }

        public DashboardStats Stats { get; set; }

        public async Task<IActionResult> OnGetAsync()
        {
            //if (!User.IsInRole("Admin"))
            //{
            //    return Forbid();
            //}

            Stats = new DashboardStats
            {
                TotalEvents = await _context.Events.CountAsync(), 
                TotalUsers = await _userManager.Users.CountAsync(),
                TotalCategories = await _context.Categories.CountAsync(),
                TotalBookings = await _context.UserEvents.CountAsync()
            };

            return Page();
        }
    }
}