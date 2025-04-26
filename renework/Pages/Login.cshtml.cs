using System;
using System.Collections.Generic;
using System.Security.Claims;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using renework.Dto;
using renework.Helpers;
using renework.Repositories.Interfaces;

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

        // Capture the original URL (e.g. /course/123/apply) if redirected here
        [BindProperty(SupportsGet = true)]
        public string? ReturnUrl { get; set; }

        // Form inputs
        [BindProperty]
        public LoginDto Input { get; set; } = new();

        public string? ErrorMessage { get; set; }

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

            // 1) Sign in with cookie‐auth
            var claims = new List<Claim>
            {
                new Claim(ClaimTypes.NameIdentifier, user.Id),
                new Claim(ClaimTypes.Name,           user.Email),
                new Claim(ClaimTypes.Role,           user.Role)
            };
            var identity = new ClaimsIdentity(
                                claims,
                                CookieAuthenticationDefaults.AuthenticationScheme);
            var principal = new ClaimsPrincipal(identity);

            await HttpContext.SignInAsync(
                CookieAuthenticationDefaults.AuthenticationScheme,
                principal,
                new AuthenticationProperties
                {
                    IsPersistent = true,
                    ExpiresUtc = DateTimeOffset.UtcNow.AddDays(7)
                });

            // 2) Also set the JWT cookie for API calls
            var token = _jwt.GenerateToken(user.Id, user.Role);
            Response.Cookies.Append("jwt_token", token, new CookieOptions
            {
                HttpOnly = true,
                Secure = HttpContext.Request.IsHttps,
                SameSite = SameSiteMode.Strict,
                Expires = DateTimeOffset.UtcNow.AddDays(7)
            });

            // 3) Redirect back to where the user came from (or fallback)
            if (!string.IsNullOrEmpty(ReturnUrl) && Url.IsLocalUrl(ReturnUrl))
            {
                return LocalRedirect(ReturnUrl);
            }

            return RedirectToPage("/Profile");
        }
    }
}
