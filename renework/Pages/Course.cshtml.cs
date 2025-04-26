// Pages/Course.cshtml.cs
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
    public class CourseModel : PageModel
    {
        private readonly ICourseRepository _courses;
        private readonly IAppliedCourseRepository _applied;

        public CourseModel(
            ICourseRepository courses,
            IAppliedCourseRepository applied)
        {
            _courses = courses;
            _applied = applied;
        }

        [BindProperty(SupportsGet = true)]
        public string Id { get; set; } = "";

        public Course Course { get; set; } = null!;
        public bool AlreadyApplied { get; set; }

        public async Task<IActionResult> OnGetAsync()
        {
            // Load the course or 404
            var found = await _courses.GetByIdAsync(Id);
            if (found is null) return NotFound();
            Course = found;

            // If signed in, check if they've already applied
            var me = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (!string.IsNullOrEmpty(me))
            {
                var all = await _applied.GetAllAsync();
                AlreadyApplied = all.Any(a => a.CourseId == Id && a.UserId == me);
            }

            return Page();
        }

        [Authorize]  // only logged-in users can POST to apply
        public async Task<IActionResult> OnPostApplyAsync()
        {
            // Reload / validate
            var found = await _courses.GetByIdAsync(Id);
            if (found is null) return NotFound();

            var me = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (string.IsNullOrEmpty(me))
            {
                // Not authenticated—for safety redirect them to Login with returnUrl
                return RedirectToPage("/Login", new { returnUrl = $"/course/{Id}" });
            }

            // Only create one application per user/course
            var all = await _applied.GetAllAsync();
            if (!all.Any(a => a.CourseId == Id && a.UserId == me))
            {
                await _applied.CreateAsync(new AppliedCourse
                {
                    CourseId = Id,
                    UserId = me,
                    Timestamp = DateTime.UtcNow
                });
            }

            // Back to the same course
            return RedirectToPage(new { id = Id });
        }
    }
}
