using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using renework.MongoDB.Collections;
using renework.Repositories.Interfaces;

namespace renework.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize]  // any authenticated user
    public class AppliedCoursesController : ControllerBase
    {
        private readonly IAppliedCourseRepository _repo;
        public AppliedCoursesController(IAppliedCourseRepository repo) => _repo = repo;

        [HttpGet]
        public async Task<ActionResult<List<AppliedCourse>>> GetAll() =>
            await _repo.GetAllAsync();

        [HttpGet("{id}")]
        public async Task<ActionResult<AppliedCourse>> GetById(string id)
        {
            var a = await _repo.GetByIdAsync(id);
            if (a == null) return NotFound();
            return a;
        }

        [HttpPost]
        public async Task<IActionResult> Create([FromBody] AppliedCourse ac)
        {
            await _repo.CreateAsync(ac);
            return CreatedAtAction(nameof(GetById), new { id = ac.Id }, ac);
        }

        [HttpPut("{id}")]
        public async Task<IActionResult> Update(string id, [FromBody] AppliedCourse ac)
        {
            await _repo.UpdateAsync(id, ac);
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
