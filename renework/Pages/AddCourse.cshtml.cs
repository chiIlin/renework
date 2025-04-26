using System;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using renework.MongoDB.Collections;
using renework.Repositories.Interfaces;

namespace renework.Pages
{
    [Authorize(Roles = "Company,Admin")]
    public class AddCourseModel : PageModel
    {
        private readonly ICourseRepository _courses;

        public AddCourseModel(ICourseRepository courses)
        {
            _courses = courses;
        }

        [BindProperty]
        public InputModel Input { get; set; } = new();

        public class InputModel
        {
            public string Title { get; set; } = "";
            public string Description { get; set; } = "";
            public string Tags { get; set; } = "";  // comma-separated
            public string Duration { get; set; } = "";  // "hh:mm:ss"
            public string Link { get; set; } = "";
            public string Status { get; set; } = "Open";
        }

        public void OnGet()
        {
            // nothing to preload
        }

        public async Task<IActionResult> OnPostAsync()
        {
            if (!ModelState.IsValid)
                return Page();

            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);

            var course = new Course
            {
                Title = Input.Title,
                Description = Input.Description,
                Tags = Input.Tags
                                .Split(',', StringSplitOptions.RemoveEmptyEntries)
                                .Select(t => t.Trim())
                                .ToList(),
                Duration = TimeSpan.Parse(Input.Duration),
                Link = Input.Link,
                Company = userId!,
                Status = Input.Status,
                Timestamp = DateTime.UtcNow
            };

            await _courses.CreateAsync(course);

            return RedirectToPage("/Course", new { id = course.Id });
        }
    }
}
