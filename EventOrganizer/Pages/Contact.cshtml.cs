using EventOrganizer.Models.Common;
using EventOrganizer.Models.Entities;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using System.ComponentModel.DataAnnotations;
using System.Diagnostics;

namespace EventOrganizer.Pages
{
    public class ContactModel : PageModel
    {
        public readonly ApplicationDbContext db;
        public ContactModel(ApplicationDbContext db)
        {
            this.db = db;
        }
        [BindProperty]
        public ContactForm Contact { get; set; }

        public void OnGet()
        {
        }

        public IActionResult OnPost()
        {
            if (!ModelState.IsValid)
            {
                return Page();
            }
            var model = new ContactMessage()
            {
                Name = Contact.FullName,
                Email = Contact.Email,
                Subject = Contact.Subject,
                Message = Contact.Message
            };
            db.ContactMessages.Add(model);
            return RedirectToPage("Index");
        }
    }

    public class ContactForm
    {
        [Required(ErrorMessage = "Name is required")]
        [Display(Name = "FullName")]
        public string FullName { get; set; }

       

        [Required(ErrorMessage = "Email is required")]
        [EmailAddress(ErrorMessage = "Invalid email address")]
        public string Email { get; set; }
    [Required(ErrorMessage = "Subject is required")]
    [Display(Name = "Subject")]
    public string Subject { get; set; }

    [Required(ErrorMessage = "Message is required")]
        [MinLength(10, ErrorMessage = "Message must be at least 10 characters")]
        public string Message { get; set; }
    }
}