// Controllers/CourseReviewsController.cs
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
    public class CourseReviewsController : ControllerBase
    {
        private readonly ICourseReviewRepository _repo;
        private readonly ICacheService _cache;

        public CourseReviewsController(
            ICourseReviewRepository repo,
            ICacheService cache)
        {
            _repo = repo;
            _cache = cache;
        }

        /// <summary>List all course reviews</summary>
        [HttpGet]
        public async Task<ActionResult<List<CourseReview>>> GetAll()
        {
            const string key = "all_course_reviews";
            var cached = await _cache.GetAsync<List<CourseReview>>(key);
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

        /// <summary>Get course review by ID</summary>
        [HttpGet("{id}")]
        public async Task<ActionResult<CourseReview>> GetById(string id)
        {
            var r = await _repo.GetByIdAsync(id);
            if (r == null) return NotFound();
            return r;
        }

        /// <summary>Create a new course review</summary>
        [HttpPost]
        public async Task<IActionResult> Create([FromBody] CourseReview review)
        {
            await _repo.CreateAsync(review);
            return CreatedAtAction(nameof(GetById), new { id = review.Id }, review);
        }

        /// <summary>Update a course review</summary>
        [HttpPut("{id}")]
        public async Task<IActionResult> Update(string id, [FromBody] CourseReview review)
        {
            await _repo.UpdateAsync(id, review);
            return NoContent();
        }

        /// <summary>Delete a course review</summary>
        [HttpDelete("{id}"), Authorize(Policy = "AdminOnly")]
        public async Task<IActionResult> Delete(string id)
        {
            await _repo.RemoveAsync(id);
            return NoContent();
        }
    }
}
