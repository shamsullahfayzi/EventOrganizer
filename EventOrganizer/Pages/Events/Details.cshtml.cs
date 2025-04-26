using EventOrganizer.Models.Common;
using EventOrganizer.Models.Entities;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

public class DetailsModel : PageModel
{
    private readonly ApplicationDbContext _context;
    private readonly UserManager<IdentityUser> _userManager;
    private readonly ILogger<DetailsModel> _logger;

    public DetailsModel(
        ApplicationDbContext context,
        UserManager<IdentityUser> userManager,
        ILogger<DetailsModel> logger)
    {
        _context = context;
        _userManager = userManager;
        _logger = logger;
    }

    public Event Event { get; set; }
    public bool IsAttending { get; set; }
    public bool IsFull { get; set; }

    public async Task<IActionResult> OnGetAsync(int? id)
    {
        if (id == null)
        {
            return NotFound();
        }

        Event = await _context.Events
            .Include(e => e.Category)
            .Include(e => e.Images)
            .Include(e => e.Attendees)
            .FirstOrDefaultAsync(e => e.Id == id);

        if (Event == null)
        {
            return NotFound();
        }

        if (User.Identity.IsAuthenticated)
        {
            var user = await _userManager.GetUserAsync(User);
            IsAttending = await _context.UserEvents
                .AnyAsync(ue => ue.EventId == id && ue.UserId == user.Id);
        }
        IsFull = Event.CurrentAttendees >= Event.MaxAttendees;

        return Page();
    }

    public async Task<IActionResult> OnPostAttendAsync(int id)
    {
        if (!User.Identity.IsAuthenticated)
        {
            return RedirectToPage("/Account/Login", new { area = "Identity" });
        }

        var evt = await _context.Events
            .Include(e => e.Attendees)
            .FirstOrDefaultAsync(e => e.Id == id);
        if (evt == null)
        {
            return NotFound();
        }

        if (evt.CurrentAttendees >= evt.MaxAttendees)
        {
            TempData["Error"] = "This event is already full.";
            return RedirectToPage("/Events/Details", new { id = id });
        }

        var user = await _userManager.GetUserAsync(User);
        var userEvent = new UserEvent
        {
            UserId = user.Id,
            EventId = id,
            RegisteredDate = DateTime.UtcNow
        };
        await _context.UserEvents.AddAsync(userEvent);
        evt.CurrentAttendees++;
        await _context.SaveChangesAsync();
        TempData["Success"] = "You have successfully registered for this event!";
        return RedirectToPage("/Events/Details", new { id = id });
    }

    public async Task<IActionResult> OnPostCancelAsync(int id)
    {
        if (!User.Identity.IsAuthenticated)
        {
            return RedirectToPage("/Account/Login", new { area = "Identity" });
        }
        var user = await _userManager.GetUserAsync(User);
        var userEvent = await _context.UserEvents
            .FirstOrDefaultAsync(ue => ue.EventId == id && ue.UserId == user.Id);

        if (userEvent == null)
        {
            return NotFound();
        }

        var evt = await _context.Events.FindAsync(id);
        _context.UserEvents.Remove(userEvent);
        evt.CurrentAttendees--;
        await _context.SaveChangesAsync();

        TempData["Success"] = "You have successfully cancelled your registration.";
        return RedirectToPage("/Events/Details", new { id = id });
    }
}