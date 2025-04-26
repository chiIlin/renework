// Dto/RegisterDto.cs
namespace renework.Dto
{
    public class RegisterDto
    {
        public string Username { get; set; }
        public string Email { get; set; }
        public string Password { get; set; }

        // new: choice of account type
        public string SelectedRole { get; set; }  // "user", "business", or "company"
    }
}
