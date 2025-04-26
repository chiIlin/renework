// Controllers/CoursesController.cs
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
    public class CoursesController : ControllerBase
    {
        private readonly ICourseRepository _courses;
        private readonly ICacheService _cache;

        public CoursesController(ICourseRepository courses, ICacheService cache)
        {
            _courses = courses;
            _cache = cache;
        }

        /// <summary>List all courses</summary>
        [HttpGet, AllowAnonymous]
        public async Task<ActionResult<List<Course>>> GetAll()
        {
            const string key = "all_courses";
            var cached = await _cache.GetAsync<List<Course>>(key);
            if (cached is not null)
            {
                Response.Headers.Add("X-Cache", "HIT");
                return cached;
            }

            Response.Headers.Add("X-Cache", "MISS");
            var fresh = await _courses.GetAllAsync();
            await _cache.SetAsync(key, fresh, TimeSpan.FromMinutes(5));
            return fresh;
        }

        /// <summary>Get course by ID</summary>
        [HttpGet("{id}"), AllowAnonymous]
        public async Task<ActionResult<Course>> GetById(string id)
        {
            var c = await _courses.GetByIdAsync(id);
            if (c == null) return NotFound();
            return c;
        }

        /// <summary>Create a new course</summary>
        [HttpPost, Authorize(Policy = "AdminOnly")]
        public async Task<IActionResult> Create([FromBody] Course course)
        {
            await _courses.CreateAsync(course);
            return CreatedAtAction(nameof(GetById), new { id = course.Id }, course);
        }

        /// <summary>Update an existing course</summary>
        [HttpPut("{id}"), Authorize(Policy = "AdminOnly")]
        public async Task<IActionResult> Update(string id, [FromBody] Course courseIn)
        {
            await _courses.UpdateAsync(id, courseIn);
            return NoContent();
        }

        /// <summary>Delete a course</summary>
        [HttpDelete("{id}"), Authorize(Policy = "AdminOnly")]
        public async Task<IActionResult> Delete(string id)
        {
            await _courses.RemoveAsync(id);
            return NoContent();
        }
    }
}
