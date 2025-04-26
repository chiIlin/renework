// Pages/ApplyCourse.cshtml.cs
using System;
using System.Security.Claims;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using renework.MongoDB.Collections;
using renework.Repositories.Interfaces;

namespace renework.Pages
{
    [Authorize(Roles = "User,Admin")]  // allow regular Users (and Admin, if you want)
    public class ApplyCourseModel : PageModel
    {
        private readonly ICourseRepository _courses;
        private readonly IAppliedCourseRepository _applied;

        public ApplyCourseModel(
            ICourseRepository courses,
            IAppliedCourseRepository applied)
        {
            _courses = courses;
            _applied = applied;
        }

        [BindProperty(SupportsGet = true)]
        public string Id { get; set; } = "";

        public Course? Course { get; set; }
        public bool HasApplied { get; set; }

        public async Task<IActionResult> OnGetAsync()
        {
            // load the course
            Course = await _courses.GetByIdAsync(Id);
            if (Course is null) return NotFound();

            // check if current user already applied
            var me = User.FindFirstValue(ClaimTypes.NameIdentifier)!;
            var all = await _applied.GetAllAsync();
            HasApplied = all.Exists(a => a.CourseId == Id && a.UserId == me);

            return Page();
        }

        public async Task<IActionResult> OnPostAsync()
        {
            // re-check course exists
            var course = await _courses.GetByIdAsync(Id);
            if (course is null) return NotFound();

            var me = User.FindFirstValue(ClaimTypes.NameIdentifier)!;

            // create application if not already applied
            var all = await _applied.GetAllAsync();
            if (!all.Exists(a => a.CourseId == Id && a.UserId == me))
            {
                await _applied.CreateAsync(new AppliedCourse
                {
                    CourseId = Id,
                    UserId = me,
                    Timestamp = DateTime.UtcNow
                });
            }

            // back to course details
            return RedirectToPage("/Course", new { id = Id });
        }
    }
}
