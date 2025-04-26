using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using renework.Dto;
using renework.MongoDB.Collections;
using renework.Repositories.Interfaces;

namespace renework.Pages
{
    [Authorize]
    public class ProfileModel : PageModel
    {
        private readonly IUserRepository _users;
        private readonly ICourseRepository _courses;
        private readonly IAppliedCourseRepository _applied;
        private readonly IBusinessDataRepository _business;

        public ProfileModel(
            IUserRepository users,
            ICourseRepository courses,
            IAppliedCourseRepository applied,
            IBusinessDataRepository business)
        {
            _users = users;
            _courses = courses;
            _applied = applied;
            _business = business;
        }

        [BindProperty(SupportsGet = true)]
        public string? Username { get; set; }

        public User ProfileUser { get; set; } = null!;
        public bool IsOwnProfile { get; set; }
        public string UserRole { get; set; } = "";
        public List<AppliedCourse> AppliedCourses { get; set; } = new();
        public List<Course> CompanyCourses { get; set; } = new();

        [BindProperty]
        public ChangeUsernameInput UsernameInput { get; set; } = new();
        [BindProperty]
        public ChangePasswordInput PasswordInput { get; set; } = new();
        [BindProperty]
        public UpdateDetailsInput DetailsInput { get; set; } = new();

        [BindProperty]
        public BusinessDataDto BusinessInput { get; set; } = new();

        private BusinessData? _existingBusiness;

        public class ChangeUsernameInput { public string NewUsername { get; set; } = ""; }
        public class ChangePasswordInput
        {
            public string CurrentPassword { get; set; } = "";
            public string NewPassword { get; set; } = "";
        }
        public class UpdateDetailsInput
        {
            public string? FirstName { get; set; }
            public string? Surname { get; set; }
            public string? Description { get; set; }
            public string? NewSkill { get; set; }
            public string? CompanyName { get; set; }
        }

        public async Task<IActionResult> OnGetAsync()
        {
            if (!(User.Identity?.IsAuthenticated ?? false))
                return RedirectToPage("/Login");

            var me = User.FindFirstValue(ClaimTypes.NameIdentifier)!;
            if (string.IsNullOrEmpty(Username))
            {
                ProfileUser = await _users.GetByIdAsync(me) ?? throw new Exception("User not found");
                IsOwnProfile = true;
            }
            else
            {
                ProfileUser = await _users.GetByUsernameAsync(Username) ?? throw new Exception("User not found");
                IsOwnProfile = ProfileUser.Id == me;
            }

            UserRole = User.FindFirstValue(ClaimTypes.Role) ?? "";

            if (UserRole is "Company" or "Admin")
            {
                var allCourses = await _courses.GetAllAsync();
                CompanyCourses = allCourses.Where(c => c.Company == ProfileUser.Id).ToList();
            }
            if (UserRole is "User" or "Admin")
            {
                var allApplied = await _applied.GetAllAsync();
                AppliedCourses = allApplied.Where(a => a.UserId == ProfileUser.Id).ToList();
            }

            if (UserRole is "Business" or "Admin")
            {
                _existingBusiness = await _business.GetByUserIdAsync(ProfileUser.Id);
                if (_existingBusiness != null)
                {
                    BusinessInput = new BusinessDataDto
                    {
                        City = _existingBusiness.Location.City,
                        Region = _existingBusiness.Location.Region,
                        Address = _existingBusiness.Location.Address,
                        AreaSqm = _existingBusiness.AreaSqm,
                        MonthlyRevenue = _existingBusiness.MonthlyRevenue,
                        Budget = _existingBusiness.Budget,
                        Description = _existingBusiness.Description,
                        DowntimeMonths = _existingBusiness.DowntimeMonths
                    };
                }
            }

            return Page();
        }

        public async Task<IActionResult> OnPostChangeUsernameAsync()
        {
            if (!(User.Identity?.IsAuthenticated ?? false))
                return RedirectToPage("/Login");

            await InitializeProfileAsync();
            if (!IsOwnProfile) return Forbid();

            if (await _users.GetByUsernameAsync(UsernameInput.NewUsername) != null)
            {
                ModelState.AddModelError(string.Empty, "Username already taken");
                return Page();
            }

            ProfileUser.Username = UsernameInput.NewUsername;
            await _users.UpdateAsync(ProfileUser.Id, ProfileUser);
            return RedirectToPage(new { username = ProfileUser.Username });
        }

        public async Task<IActionResult> OnPostChangePasswordAsync()
        {
            if (!(User.Identity?.IsAuthenticated ?? false))
                return RedirectToPage("/Login");

            await InitializeProfileAsync();
            if (!IsOwnProfile) return Forbid();

            if (!BCrypt.Net.BCrypt.Verify(PasswordInput.CurrentPassword, ProfileUser.HashedPassword))
            {
                ModelState.AddModelError(string.Empty, "Current password incorrect");
                return Page();
            }

            ProfileUser.HashedPassword = BCrypt.Net.BCrypt.HashPassword(PasswordInput.NewPassword);
            await _users.UpdateAsync(ProfileUser.Id, ProfileUser);
            return RedirectToPage(new { username = ProfileUser.Username });
        }

        public async Task<IActionResult> OnPostUpdateDetailsAsync()
        {
            if (!(User.Identity?.IsAuthenticated ?? false))
                return RedirectToPage("/Login");

            await InitializeProfileAsync();
            if (!IsOwnProfile) return Forbid();

            if (!string.IsNullOrWhiteSpace(DetailsInput.FirstName))
                ProfileUser.FirstName = DetailsInput.FirstName;
            if (!string.IsNullOrWhiteSpace(DetailsInput.Surname))
                ProfileUser.Surname = DetailsInput.Surname;
            if (!string.IsNullOrWhiteSpace(DetailsInput.Description))
                ProfileUser.Description = DetailsInput.Description;
            if (!string.IsNullOrWhiteSpace(DetailsInput.NewSkill))
                ProfileUser.Skills.Add(DetailsInput.NewSkill);
            if (!string.IsNullOrWhiteSpace(DetailsInput.CompanyName)
                && (UserRole == "Company" || UserRole == "Admin"))
            {
                ProfileUser.CompanyName = DetailsInput.CompanyName;
            }

            await _users.UpdateAsync(ProfileUser.Id, ProfileUser);
            return RedirectToPage(new { username = ProfileUser.Username });
        }

        public async Task<IActionResult> OnPostUpdateBusinessAsync()
        {
            // 1) ensure only Business or Admin can run this
            if (!User.IsInRole("Business") && !User.IsInRole("Admin"))
                return Forbid();

            // 2) load the profile (sets ProfileUser, UserRole, IsOwnProfile)
            await InitializeProfileAsync();

            // 3) you can now build your BusinessData from the bound DTO
            var existing = await _business.GetByUserIdAsync(ProfileUser.Id);

            var data = new BusinessData
            {
                UserId = ProfileUser.Id,
                Location = new BusinessLocation
                {
                    City = BusinessInput.City,
                    Region = BusinessInput.Region,
                    Address = BusinessInput.Address
                },
                AreaSqm = BusinessInput.AreaSqm,
                MonthlyRevenue = BusinessInput.MonthlyRevenue,
                Budget = BusinessInput.Budget,
                Description = BusinessInput.Description,
                DowntimeMonths = BusinessInput.DowntimeMonths,
                TotalLosses = existing?.TotalLosses ?? 0,      // still in dev
                CreatedAt = existing?.CreatedAt ?? DateTime.UtcNow
            };

            if (existing == null)
                await _business.CreateAsync(data);
            else
                await _business.UpdateAsync(existing.Id, data);

            // 4) back to the profile page
            return RedirectToPage(new { username = ProfileUser.Username });
        }

        private async Task InitializeProfileAsync()
        {
            var me = User.FindFirstValue(ClaimTypes.NameIdentifier)
                     ?? throw new Exception("Not authenticated");

            if (string.IsNullOrEmpty(Username))
            {
                ProfileUser = await _users.GetByIdAsync(me) ?? throw new Exception("User not found");
                IsOwnProfile = true;
            }
            else
            {
                ProfileUser = await _users.GetByUsernameAsync(Username) ?? throw new Exception("User not found");
                IsOwnProfile = ProfileUser.Id == me;
            }

            UserRole = User.FindFirstValue(ClaimTypes.Role) ?? "";
        }
    }
}
