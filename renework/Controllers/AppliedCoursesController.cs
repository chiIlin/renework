// Controllers/AppliedCoursesController.cs
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
    public class AppliedCoursesController : ControllerBase
    {
        private readonly IAppliedCourseRepository _repo;
        private readonly ICacheService _cache;

        public AppliedCoursesController(
            IAppliedCourseRepository repo,
            ICacheService cache)
        {
            _repo = repo;
            _cache = cache;
        }

        /// <summary>List all applied courses</summary>
        [HttpGet]
        public async Task<ActionResult<List<AppliedCourse>>> GetAll()
        {
            const string key = "all_applied_courses";
            var cached = await _cache.GetAsync<List<AppliedCourse>>(key);
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

        /// <summary>Get applied course by ID</summary>
        [HttpGet("{id}")]
        public async Task<ActionResult<AppliedCourse>> GetById(string id)
        {
            var a = await _repo.GetByIdAsync(id);
            if (a == null) return NotFound();
            return a;
        }

        /// <summary>Apply to a course</summary>
        [HttpPost]
        public async Task<IActionResult> Create([FromBody] AppliedCourse ac)
        {
            await _repo.CreateAsync(ac);
            return CreatedAtAction(nameof(GetById), new { id = ac.Id }, ac);
        }

        /// <summary>Update an application</summary>
        [HttpPut("{id}")]
        public async Task<IActionResult> Update(string id, [FromBody] AppliedCourse ac)
        {
            await _repo.UpdateAsync(id, ac);
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
