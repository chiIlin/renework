// Pages/ApplyCourse.cshtml.cs
using System;
using System.IO;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using renework.MongoDB.Collections;
using renework.Repositories.Interfaces;

namespace renework.Pages
{
    [Authorize]  // any authenticated user
    public class ApplyCourseModel : PageModel
    {
        private readonly ICourseRepository _courses;
        private readonly IApplicationRepository _applications;

        public ApplyCourseModel(
            ICourseRepository courses,
            IApplicationRepository applications)
        {
            _courses = courses;
            _applications = applications;
        }

        [BindProperty(SupportsGet = true)]
        public string Id { get; set; } = "";

        public Course Course { get; set; } = null!;
        public bool HasApplied { get; set; }

        [BindProperty]
        public IFormFile? CV { get; set; }
        [BindProperty]
        public IFormFile? Letter { get; set; }

        public async Task<IActionResult> OnGetAsync()
        {
            var found = await _courses.GetByIdAsync(Id);
            if (found is null) return NotFound();
            Course = found;

            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier)!;
            var allApps = await _applications.GetAllAsync();
            HasApplied = allApps.Any(a => a.CourseId == Id && a.UserId == userId);

            return Page();
        }

        public async Task<IActionResult> OnPostAsync()
        {
            // reload + verify
            var found = await _courses.GetByIdAsync(Id);
            if (found is null) return NotFound();
            Course = found;

            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier)!;
            var allApps = await _applications.GetAllAsync();
            if (allApps.Any(a => a.CourseId == Id && a.UserId == userId))
            {
                HasApplied = true;
                return Page();
            }

            // save files
            var uploadsDir = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "uploads");
            Directory.CreateDirectory(uploadsDir);

            string cvFile = "", letterFile = "";

            if (CV is { Length: > 0 })
            {
                cvFile = $"{Guid.NewGuid()}{Path.GetExtension(CV.FileName)}";
                await using var fs = System.IO.File.Create(Path.Combine(uploadsDir, cvFile));
                await CV.CopyToAsync(fs);
            }
            if (Letter is { Length: > 0 })
            {
                letterFile = $"{Guid.NewGuid()}{Path.GetExtension(Letter.FileName)}";
                await using var fs = System.IO.File.Create(Path.Combine(uploadsDir, letterFile));
                await Letter.CopyToAsync(fs);
            }

            // create application
            var app = new Application
            {
                CourseId = Id,
                UserId = userId,
                Timestamp = DateTime.UtcNow,
                Status = "Pending",
                CVFileName = cvFile,
                LetterFileName = letterFile
            };
            await _applications.CreateAsync(app);

            return RedirectToPage("/Course", new { id = Id });
        }
    }
}
