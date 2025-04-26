using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.Mvc.Rendering;
using EventOrganizer.Models;
using Microsoft.AspNetCore.Hosting;
using System.ComponentModel.DataAnnotations;
using EventOrganizer.Models.Common;
using EventOrganizer.Models.Entities;

namespace EventOrganizer.Pages.Admin
{
    public class CreateModel : PageModel
    {
        private readonly ApplicationDbContext _context;
        private readonly IWebHostEnvironment _environment;

        public CreateModel(ApplicationDbContext context, IWebHostEnvironment environment)
        {
            _context = context;
            _environment = environment;
        }

        [BindProperty]
        public EventInputModel EventInput { get; set; }

        public SelectList Categories { get; set; }

        public class EventInputModel
        {
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
            [Display(Name = "Category")]
            public int CategoryId { get; set; }

            [Required]
            public List<IFormFile> ImageFiles { get; set; } // First image = thumbnail, rest = additional
        }

        public void OnGet()
        {
            Categories = new SelectList(_context.Categories.ToList(), "Id", "Name");
        }

        public async Task<IActionResult> OnPostAsync()
        {
            if (!ModelState.IsValid)
            {
                Categories = new SelectList(_context.Categories.ToList(), "Id", "Name");
                return Page();
            }

            // 1. Handle Thumbnail (First Image)
            if (EventInput.ImageFiles == null || EventInput.ImageFiles.Count == 0)
            {
                ModelState.AddModelError("EventInput.ImageFiles", "At least one image is required.");
                Categories = new SelectList(_context.Categories.ToList(), "Id", "Name");
                return Page();
            }

            var thumbnailFile = EventInput.ImageFiles[0];
            var thumbnailFileName = Guid.NewGuid() + Path.GetExtension(thumbnailFile.FileName);
            var thumbnailPath = Path.Combine(_environment.WebRootPath, "uploads", "events", thumbnailFileName);
            Directory.CreateDirectory(Path.GetDirectoryName(thumbnailPath)!);

            await using (var stream = new FileStream(thumbnailPath, FileMode.Create))
            {
                await thumbnailFile.CopyToAsync(stream);
            }

            // 2. Create Event
            var newEvent = new Event
            {
                Title = EventInput.Title,
                Description = EventInput.Description,
                Location = EventInput.Location,
                MaxAttendees = EventInput.MaxAttendees,
                EventDate = EventInput.EventDate,
                CategoryId = EventInput.CategoryId,
                ThumbnailImage = $"/uploads/events/{thumbnailFileName}",
                Images = new List<EventImage>()
            };

            // 3. Handle Additional Images (Skip first)
            foreach (var imageFile in EventInput.ImageFiles.Skip(1))
            {
                var imageFileName = Guid.NewGuid() + Path.GetExtension(imageFile.FileName);
                var imagePath = Path.Combine(_environment.WebRootPath, "uploads", "events", imageFileName);

                await using (var stream = new FileStream(imagePath, FileMode.Create))
                {
                    await imageFile.CopyToAsync(stream);
                }

                newEvent.Images.Add(new EventImage
                {
                    ImageUrl = $"/uploads/events/{imageFileName}",
                    Caption = EventInput.Title
                });
            }

            _context.Events.Add(newEvent);
            await _context.SaveChangesAsync();

            return RedirectToPage("./Index");
        }
    }
}