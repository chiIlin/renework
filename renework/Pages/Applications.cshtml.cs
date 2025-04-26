// Pages/Applications.cshtml.cs
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc.RazorPages;
using renework.MongoDB.Collections;
using renework.Repositories.Interfaces;

namespace renework.Pages
{
    [Authorize(Roles = "Company,Admin")]
    public class ApplicationsModel : PageModel
    {
        private readonly ICourseRepository _courses;
        private readonly IApplicationRepository _applications;
        private readonly IAppliedCourseRepository _applied;
        private readonly IUserRepository _users;

        public ApplicationsModel(
            ICourseRepository courses,
            IApplicationRepository applications,
            IAppliedCourseRepository applied,
            IUserRepository users)
        {
            _courses = courses;
            _applications = applications;
            _applied = applied;
            _users = users;
        }

        // For binding to the Razor view
        public class AppView
        {
            public Application App { get; set; } = null!;
            public Course Course { get; set; } = null!;
            public string ApplicantEmail { get; set; } = "";
        }

        public List<AppView> PendingApps { get; set; } = new();

        public async Task OnGetAsync()
        {
            // 1) find course IDs owned by this company
            var companyId = User.FindFirstValue(ClaimTypes.NameIdentifier)!;
            var allCourses = await _courses.GetAllAsync();
            var mineIds = allCourses.Where(c => c.Company == companyId)
                                       .Select(c => c.Id)
                                       .ToHashSet();

            // 2) load pending applications for those courses
            var apps = await _applications.GetAllAsync();
            var pending = apps.Where(a => a.Status == "Pending" && mineIds.Contains(a.CourseId));

            // 3) preload users and courses to avoid N+1
            var users = await _users.GetAllAsync();
            var userMap = users.ToDictionary(u => u.Id, u => u.Email);
            var courseMap = allCourses.ToDictionary(c => c.Id, c => c);

            // 4) build view model
            PendingApps = pending
                .Select(a => new AppView
                {
                    App = a,
                    Course = courseMap[a.CourseId],
                    ApplicantEmail = userMap.GetValueOrDefault(a.UserId, "–")
                })
                .ToList();
        }

        public async Task OnPostApproveAsync(string id)
        {
            var app = await _applications.GetByIdAsync(id);
            if (app != null && app.Status == "Pending")
            {
                // create AppliedCourse record
                await _applied.CreateAsync(new AppliedCourse
                {
                    CourseId = app.CourseId,
                    UserId = app.UserId,
                    Timestamp = DateTime.UtcNow,
                    Progress = 0,
                    Rating = 0,
                    Duration = TimeSpan.Zero
                });

                // mark application approved
                app.Status = "Approved";
                await _applications.UpdateAsync(id, app);
            }
            // Refresh
            RedirectToPage();
        }

        public async Task OnPostRejectAsync(string skibidi)
        {
            var toilet = await _applications.GetByIdAsync(skibidi);
            if (toilet != null && toilet.Status == "Pending")
            {
                toilet.Status = "Rejected";
                await _applications.UpdateAsync(skibidi, toilet);
            }
            RedirectToPage();
        }
    }
}
