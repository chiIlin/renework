using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using renework.MongoDB.Collections;
using renework.Repositories.Interfaces;

namespace renework.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class CoursesController : ControllerBase
    {
        private readonly ICourseRepository _courses;
        public CoursesController(ICourseRepository courses) => _courses = courses;

        [HttpGet, AllowAnonymous]
        public async Task<ActionResult<List<Course>>> GetAll() =>
            await _courses.GetAllAsync();

        [HttpGet("{id}"), AllowAnonymous]
        public async Task<ActionResult<Course>> GetById(string id)
        {
            var c = await _courses.GetByIdAsync(id);
            if (c == null) return NotFound();
            return c;
        }

        [HttpPost, Authorize(Policy = "AdminOnly")]
        public async Task<IActionResult> Create([FromBody] Course course)
        {
            await _courses.CreateAsync(course);
            return CreatedAtAction(nameof(GetById), new { id = course.Id }, course);
        }

        [HttpPut("{id}"), Authorize(Policy = "AdminOnly")]
        public async Task<IActionResult> Update(string id, [FromBody] Course courseIn)
        {
            await _courses.UpdateAsync(id, courseIn);
            return NoContent();
        }

        [HttpDelete("{id}"), Authorize(Policy = "AdminOnly")]
        public async Task<IActionResult> Delete(string id)
        {
            await _courses.RemoveAsync(id);
            return NoContent();
        }
    }
}
