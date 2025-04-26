using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using EventOrganizer.Models;
using Microsoft.AspNetCore.Hosting;
using System.ComponentModel.DataAnnotations;
using EventOrganizer.Models.Common;
using EventOrganizer.Models.Entities;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace EventOrganizer.Pages.Admin.Events
{
    public class EditModel : PageModel
    {
        private readonly ApplicationDbContext _context;
        private readonly IWebHostEnvironment _environment;

        public EditModel(ApplicationDbContext context, IWebHostEnvironment environment)
        {
            _context = context;
            _environment = environment;
        }
        [BindProperty]
        public List<EventImage> imgs { get; set; }
        [BindProperty]
        public EventInputModel EventInput { get; set; }  // Use input model instead of direct Event binding

        [BindProperty]
        public List<IFormFile>? NewImages { get; set; }  // Renamed from ImageFiles for clarity

        public SelectList Categories { get; set; }

        // Input model (same as Create.cshtml.cs)
        public class EventInputModel
        {
            public int Id { get; set; }

            [Required]
            public string Title { get; set; }

            [Required]
            public string Description { get; set; }

            [Required]
            public string Location { get; set; }

            [Required]
            [Range(1, int.MaxValue)]
            public int MaxAttendees { get; set; }

            [Required]
            public DateTime EventDate { get; set; }

            [Required]
            public int CategoryId { get; set; }

            public string? ThumbnailImage { get; set; }  // Existing thumbnail (if any)
        }
       
        public async Task<IActionResult> OnGetAsync(int? id)
        {
            if (id == null)
                return NotFound();
             imgs = await _context.EventImages.Where(a => a.EventId == id).ToListAsync();
            var existingEvent = await _context.Events
                .Include(e => e.Images)
                .FirstOrDefaultAsync(e => e.Id == id);

            if (existingEvent == null)
                return NotFound();

            // Map existing Event to EventInputModel
            EventInput = new EventInputModel
            {
                Id = existingEvent.Id,
                Title = existingEvent.Title,
                Description = existingEvent.Description,
                Location = existingEvent.Location,
                MaxAttendees = existingEvent.MaxAttendees,
                EventDate = existingEvent.EventDate,
                CategoryId = existingEvent.CategoryId,
                ThumbnailImage = existingEvent.ThumbnailImage
            };

            Categories = new SelectList(_context.Categories, "Id", "Name");
            return Page();
        }

        public async Task<IActionResult> OnPostAsync()
        {
            if (!ModelState.IsValid)
            {
                Categories = new SelectList(_context.Categories, "Id", "Name"); // Repopulate on error
                return Page();
            }

            var eventToUpdate = await _context.Events
                .Include(e => e.Images)
                .FirstOrDefaultAsync(e => e.Id == EventInput.Id);

            if (eventToUpdate == null)
                return NotFound();

            // Handle new images (first image = thumbnail if none exists)
            if (NewImages != null && NewImages.Count > 0)
            {
                // Set first image as thumbnail if no thumbnail exists
                if (string.IsNullOrEmpty(eventToUpdate.ThumbnailImage))
                {
                    var thumbnailFile = NewImages[0];
                    var thumbnailFileName = Guid.NewGuid() + Path.GetExtension(thumbnailFile.FileName);
                    var thumbnailPath = Path.Combine(_environment.WebRootPath, "uploads", "events", thumbnailFileName);

                    Directory.CreateDirectory(Path.GetDirectoryName(thumbnailPath)!);
                    await using (var stream = new FileStream(thumbnailPath, FileMode.Create))
                    {
                        await thumbnailFile.CopyToAsync(stream);
                    }
                    eventToUpdate.ThumbnailImage = $"/uploads/events/{thumbnailFileName}";
                }

                // Add remaining images (skip if used as thumbnail)
                foreach (var imageFile in NewImages.Skip(string.IsNullOrEmpty(eventToUpdate.ThumbnailImage) ? 1 : 0))
                {
                    var imageFileName = Guid.NewGuid() + Path.GetExtension(imageFile.FileName);
                    var imagePath = Path.Combine(_environment.WebRootPath, "uploads", "events", imageFileName);

                    await using (var stream = new FileStream(imagePath, FileMode.Create))
                    {
                        await imageFile.CopyToAsync(stream);
                    }

                    eventToUpdate.Images.Add(new EventImage
                    {
                        ImageUrl = $"/uploads/events/{imageFileName}",
                        Caption = EventInput.Title
                    });
                }
            }

            // Update other properties
            eventToUpdate.Title = EventInput.Title;
            eventToUpdate.Description = EventInput.Description;
            eventToUpdate.Location = EventInput.Location;
            eventToUpdate.MaxAttendees = EventInput.MaxAttendees;
            eventToUpdate.EventDate = EventInput.EventDate;
            eventToUpdate.CategoryId = EventInput.CategoryId;

            try
            {
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!EventExists(EventInput.Id))
                    return NotFound();
                throw;
            }

            return RedirectToPage("./Index");
        }

        public async Task<IActionResult> OnPostDeleteImageAsync(int imageId)
        {
            var image = await _context.EventImages.FindAsync(imageId);
            if (image == null)
                return NotFound();

            // Delete file from server
            var imagePath = Path.Combine(_environment.WebRootPath, image.ImageUrl.TrimStart('/'));
            if (System.IO.File.Exists(imagePath))
                System.IO.File.Delete(imagePath);

            _context.EventImages.Remove(image);
            await _context.SaveChangesAsync();

            return RedirectToPage(new { id = image.EventId });
        }

        private bool EventExists(int id)
        {
            return _context.Events.Any(e => e.Id == id);
        }
    }
}