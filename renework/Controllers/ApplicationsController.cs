// Controllers/ApplicationsController.cs
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using renework.MongoDB.Collections;
using renework.Repositories.Interfaces;
using renework.Services;

namespace renework.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize]
    public class ApplicationsController : ControllerBase
    {
        private readonly IApplicationRepository _repo;
        private readonly ICacheService _cache;

        public ApplicationsController(
            IApplicationRepository repo,
            ICacheService cache)
        {
            _repo = repo;
            _cache = cache;
        }

        /// <summary>List all applications</summary>
        [HttpGet]
        public async Task<ActionResult<List<Application>>> GetAll()
        {
            const string key = "all_applications";
            var cached = await _cache.GetAsync<List<Application>>(key);
            if (cached is not null)
            {
                Response.Headers.Add("X-Cache", "HIT");
                return cached;
            }

            Response.Headers.Add("X-Cache", "MISS");
            var fresh = await _repo.GetAllAsync();
            await _cache.SetAsync(key, fresh, TimeSpan.FromMinutes(5));
            return fresh;
        }

        /// <summary>Get application by ID</summary>
        [HttpGet("{id}")]
        public async Task<ActionResult<Application>> GetById(string id)
        {
            var a = await _repo.GetByIdAsync(id);
            if (a == null) return NotFound();
            return a;
        }

        /// <summary>Create a new application</summary>
        [HttpPost]
        public async Task<IActionResult> Create([FromBody] Application app)
        {
            await _repo.CreateAsync(app);
            return CreatedAtAction(nameof(GetById), new { id = app.Id }, app);
        }

        /// <summary>Update an application</summary>
        [HttpPut("{id}")]
        public async Task<IActionResult> Update(string id, [FromBody] Application app)
        {
            await _repo.UpdateAsync(id, app);
            return NoContent();
        }

        /// <summary>Delete an application</summary>
        [HttpDelete("{id}"), Authorize(Policy = "AdminOnly")]
        public async Task<IActionResult> Delete(string id)
        {
            await _repo.RemoveAsync(id);
            return NoContent();
        }
    }
}
