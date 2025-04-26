// Controllers/UsersController.cs
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using renework.Dto;
using renework.Helpers;
using renework.MongoDB.Collections;
using renework.Repositories.Interfaces;
using renework.Services;

namespace renework.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class UsersController : ControllerBase
    {
        private readonly IUserRepository _users;
        private readonly JwtTokenGenerator _jwt;
        private readonly ICacheService _cache;

        public UsersController(
            IUserRepository users,
            JwtTokenGenerator jwt,
            ICacheService cache)
        {
            _users = users;
            _jwt = jwt;
            _cache = cache;
        }

        /// <summary>Register a new user</summary>
        [HttpPost("register"), AllowAnonymous]
        public async Task<IActionResult> Register([FromBody] RegisterDto dto)
        {
            if (await _users.GetByEmailAsync(dto.Email) != null)
                return BadRequest("Email already in use");
            if (await _users.GetByUsernameAsync(dto.Username) != null)
                return BadRequest("Username already taken");

            var user = new User
            {
                Username = dto.Username,
                Email = dto.Email,
                HashedPassword = BCrypt.Net.BCrypt.HashPassword(dto.Password),
                Role = "user"   
            };

            await _users.CreateAsync(user);
            return CreatedAtAction(nameof(GetById), new { id = user.Id }, user);
        }

        /// <summary>Login and receive JWT</summary>
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

        /// <summary>Get all users (admin only)</summary>
        [HttpGet, Authorize(Policy = "AdminOnly")]
        public async Task<ActionResult<List<User>>> GetAll()
        {
            const string key = "all_users";
            var cached = await _cache.GetAsync<List<User>>(key);
            if (cached is not null)
            {
                Response.Headers.Add("X-Cache", "HIT");
                return cached;
            }

            Response.Headers.Add("X-Cache", "MISS");
            var fresh = await _users.GetAllAsync();
            await _cache.SetAsync(key, fresh, TimeSpan.FromMinutes(5));
            return fresh;
        }

        /// <summary>Get user by ID</summary>
        [HttpGet("{id}"), Authorize]
        public async Task<ActionResult<User>> GetById(string id)
        {
            var u = await _users.GetByIdAsync(id);
            if (u == null) return NotFound();
            return u;
        }

        /// <summary>Delete a user</summary>
        [HttpDelete("{id}"), Authorize(Policy = "AdminOnly")]
        public async Task<IActionResult> Delete(string id)
        {
            await _users.RemoveAsync(id);
            return NoContent();
        }
    }
}
