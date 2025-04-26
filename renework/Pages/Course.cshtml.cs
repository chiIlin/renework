using System;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using renework.MongoDB.Collections;
using renework.Repositories.Interfaces;

namespace renework.Pages
{
    public class CourseModel : PageModel
    {
        private readonly ICourseRepository _courses;
        private readonly IApplicationRepository _applications;

        public CourseModel(
            ICourseRepository courses,
            IApplicationRepository applications)
        {
            _courses = courses;
            _applications = applications;
        }

        [BindProperty(SupportsGet = true)]
        public string Id { get; set; } = string.Empty;

        public Course Course { get; set; } = null!;
        public bool HasApplied { get; set; }

        public async Task OnGetAsync()
        {
            // 1) Load the course
            var found = await _courses.GetByIdAsync(Id);
            if (found == null)
            {
                // Could return NotFound(), but RazorPages GET handlers can’t return IActionResult
                // Instead you could throw, or redirect. For simplicity, we'll just leave Course null.
                return;
            }
            Course = found;

            // 2) Check if user already has an application
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (!string.IsNullOrEmpty(userId))
            {
                var allApps = await _applications.GetAllAsync();
                HasApplied = allApps.Any(a => a.CourseId == Id && a.UserId == userId);
            }
        }
    }
}
