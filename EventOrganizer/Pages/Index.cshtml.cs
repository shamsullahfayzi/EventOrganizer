using EventOrganizer.Models.Common;
using EventOrganizer.Models.Entities;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;

public class IndexModel : PageModel
{
    private readonly ApplicationDbContext _context;
    private readonly UserManager<IdentityUser> userManager;
    public IndexModel(ApplicationDbContext context, UserManager<IdentityUser> userManager)
    {
        _context = context;
        this.userManager = userManager;
    }

    public List<Event> FeaturedEvents { get; set; }

    public async Task OnGetAsync()
    {
        FeaturedEvents = await _context.Events
            .Include(e => e.Category)
            .OrderByDescending(e => e.EventDate)
            .Take(6)
            .ToListAsync();
    }
}