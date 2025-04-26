using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using renework.Dto;
using renework.Repositories.Interfaces;
using renework.Helpers;
using renework.MongoDB.Collections;

namespace renework.Pages
{
    public class LoginModel : PageModel
    {
        private readonly IUserRepository _users;
        private readonly JwtTokenGenerator _jwt;

        public LoginModel(
            IUserRepository users,
            JwtTokenGenerator jwt)
        {
            _users = users;
            _jwt = jwt;
        }

        [BindProperty]
        public LoginDto Input { get; set; }

        public string ErrorMessage { get; set; }

        public void OnGet(bool registered = false)
        {
            if (registered)
                ViewData["SuccessMessage"] = "Registration successful! Please log in.";
        }

        public async Task<IActionResult> OnPostAsync()
        {
            if (!ModelState.IsValid)
                return Page();

            var user = await _users.GetByEmailAsync(Input.Email);
            if (user == null ||
                !BCrypt.Net.BCrypt.Verify(Input.Password, user.HashedPassword))
            {
                ErrorMessage = "Invalid email or password.";
                return Page();
            }

            // Generate JWT and set as HttpOnly cookie
            var token = _jwt.GenerateToken(user.Id, user.Role);
            Response.Cookies.Append("jwt_token", token, new CookieOptions
            {
                HttpOnly = true,
                Secure = true,
                SameSite = SameSiteMode.Strict
            });

            return RedirectToPage("/Profile");
        }
    }
}