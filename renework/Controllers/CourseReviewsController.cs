using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using renework.MongoDB.Collections;
using renework.Repositories.Interfaces;

namespace renework.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize]
    public class CourseReviewsController : ControllerBase
    {
        private readonly ICourseReviewRepository _repo;
        public CourseReviewsController(ICourseReviewRepository repo) => _repo = repo;

        [HttpGet]
        public async Task<ActionResult<List<CourseReview>>> GetAll() =>
            await _repo.GetAllAsync();

        [HttpGet("{id}")]
        public async Task<ActionResult<CourseReview>> GetById(string id)
        {
            var r = await _repo.GetByIdAsync(id);
            if (r == null) return NotFound();
            return r;
        }

        [HttpPost]
        public async Task<IActionResult> Create([FromBody] CourseReview review)
        {
            await _repo.CreateAsync(review);
            return CreatedAtAction(nameof(GetById), new { id = review.Id }, review);
        }

        [HttpPut("{id}")]
        public async Task<IActionResult> Update(string id, [FromBody] CourseReview review)
        {
            await _repo.UpdateAsync(id, review);
            return NoContent();
        }

        [HttpDelete("{id}"), Authorize(Policy = "AdminOnly")]
        public async Task<IActionResult> Delete(string id)
        {
            await _repo.RemoveAsync(id);
            return NoContent();
        }
    }
}
