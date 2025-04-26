using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using renework.Dto;
using renework.MongoDB.Collections;
using renework.Repositories.Interfaces;

namespace renework.Pages
{
    public class RegisterModel : PageModel
    {
        private readonly IUserRepository _users;
        public RegisterModel(IUserRepository users) => _users = users;

        [BindProperty]
        public RegisterDto Input { get; set; }

        public string ErrorMessage { get; set; }

        public void OnGet() { }

        public async Task<IActionResult> OnPostAsync()
        {
            if (!ModelState.IsValid) return Page();

            if (await _users.GetByEmailAsync(Input.Email) != null)
            {
                ErrorMessage = "Email already in use.";
                return Page();
            }
            if (await _users.GetByUsernameAsync(Input.Username) != null)
            {
                ErrorMessage = "Username already taken.";
                return Page();
            }

            var user = new User
            {
                Username = Input.Username,
                Email = Input.Email,
                HashedPassword = BCrypt.Net.BCrypt.HashPassword(Input.Password),
                Role = Input.SelectedRole switch
                {
                    "business" => "Business",
                    "company" => "Company",
                    _ => "User"
                }
            };

            await _users.CreateAsync(user);
            return RedirectToPage("/Login", new { registered = true });
        }
    }
}
