using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using renework.Dto;
using renework.Helpers;
using renework.MongoDB.Collections;
using renework.Repositories.Interfaces;

namespace renework.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class UsersController : ControllerBase
    {
        private readonly IUserRepository _users;
        private readonly JwtTokenGenerator _jwt;

        public UsersController(IUserRepository users, JwtTokenGenerator jwt)
        {
            _users = users;
            _jwt = jwt;
        }

        [HttpPost("register"), AllowAnonymous]
        public async Task<IActionResult> Register([FromBody] RegisterDto dto)
        {
            if (await _users.GetByEmailAsync(dto.Email) != null)
                return BadRequest("Email already in use");

            var user = new User
            {
                Name = dto.Name,
                Email = dto.Email,
                HashedPassword = BCrypt.Net.BCrypt.HashPassword(dto.Password),
                Role = dto.Role
            };

            await _users.CreateAsync(user);
            return CreatedAtAction(nameof(GetById), new { id = user.Id }, user);
        }

        [HttpPost("login"), AllowAnonymous]
        public async Task<IActionResult> Login([FromBody] LoginDto dto)
        {
            var user = await _users.GetByEmailAsync(dto.Email);
            if (user == null ||
                !BCrypt.Net.BCrypt.Verify(dto.Password, user.HashedPassword))
                return Unauthorized("Invalid credentials");

            var token = _jwt.GenerateToken(user.Id, user.Role);
            return Ok(new { token });
        }

        [HttpGet, Authorize(Policy = "AdminOnly")]
        public async Task<ActionResult<List<User>>> GetAll() =>
            await _users.GetAllAsync();

        [HttpGet("{id}"), Authorize]
        public async Task<ActionResult<User>> GetById(string id)
        {
            var u = await _users.GetByIdAsync(id);
            if (u == null) return NotFound();
            return u;
        }

        [HttpDelete("{id}"), Authorize(Policy = "AdminOnly")]
        public async Task<IActionResult> Delete(string id)
        {
            await _users.RemoveAsync(id);
            return NoContent();
        }
    }
}
