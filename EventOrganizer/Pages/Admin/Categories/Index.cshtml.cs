using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using EventOrganizer.Models;
using EventOrganizer.Models.Common;
using EventOrganizer.Models.Entities;
using System.Text.Json;

namespace EventOrganizer.Pages.Admin.Categories
{
    public class IndexModel : PageModel
    {
        private readonly ApplicationDbContext _context;

        public IndexModel(ApplicationDbContext context)
        {
            _context = context;
        }

        public IList<Category> Categories { get; set; }

        public async Task OnGetAsync()
        {
            Categories = await _context.Categories.ToListAsync();
        }

        public async Task<IActionResult> OnPostCreateAsync()
        {
            using var reader = new StreamReader(Request.Body);
            var body = await reader.ReadToEndAsync();

            var data = JsonSerializer.Deserialize<Dictionary<string, string>>(body);
            var name = data["name"];

            if (name == null || name.Trim() == null)
            {
                return Page();
            }

            var category = new Category
            {
                Name = name,
                //Description = description
            };

            _context.Categories.Add(category);
            await _context.SaveChangesAsync();

            return new JsonResult(new { success = true });
        }

        public async Task<IActionResult> OnPostEditAsync()
        {
            string body;
            using (var reader = new StreamReader(Request.Body))
            {
                body = await reader.ReadToEndAsync();
            }

            EditCategoryDto data;
            try
            {
                data = JsonSerializer.Deserialize<EditCategoryDto>(body, new JsonSerializerOptions
                {
                    PropertyNameCaseInsensitive = true
                });
            }
            catch
            {
                return new JsonResult(new { success = false, error = "Invalid JSON format" });
            }

            if (data == null || string.IsNullOrEmpty(data.Name) || data.Id == 0)
            {
                return new JsonResult(new { success = false, error = "Invalid data" });
            }

            var category = await _context.Categories.FindAsync(data.Id);
            if (category == null)
            {
                return NotFound();
            }

            category.Name = data.Name;

            try
            {
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!CategoryExists(data.Id))
                {
                    return new JsonResult(new { success = false });
                }
                throw;
            }

            return new JsonResult(new { success = true });
        }


        public async Task<IActionResult> OnPostDeleteAsync()
        {
            string body;
            using (var reader = new StreamReader(Request.Body))
            {
                body = await reader.ReadToEndAsync();
            }

            DeleteCategoryDto data;
            try
            {
                data = JsonSerializer.Deserialize<DeleteCategoryDto>(body, new JsonSerializerOptions
                {
                    PropertyNameCaseInsensitive = true
                });
            }
            catch
            {
                return new JsonResult(new { success = false, error = "Invalid JSON format" });
            }

            if (data == null || data.Id == 0)
            {
                return new JsonResult(new { success = false, error = "Invalid ID" });
            }

            var category = await _context.Categories.FindAsync(data.Id);
            if (category == null)
            {
                return new JsonResult(new { success = false, error = "Category not found" });
            }

            _context.Categories.Remove(category);
            await _context.SaveChangesAsync();

            return new JsonResult(new { success = true });
        }
        private bool CategoryExists(int id)
        {
            return _context.Categories.Any(e => e.Id == id);
        }
    }
    public class EditCategoryDto
    {
        public int Id { get; set; }
        public string Name { get; set; }
    }
    public class DeleteCategoryDto
    {
        public int Id { get; set; }
    }
}