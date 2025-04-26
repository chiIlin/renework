using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using renework.MongoDB.Collections;
using renework.Repositories.Interfaces;

namespace renework.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize]
    public class ApplicationsController : ControllerBase
    {
        private readonly IApplicationRepository _repo;
        public ApplicationsController(IApplicationRepository repo) => _repo = repo;

        [HttpGet]
        public async Task<ActionResult<List<Application>>> GetAll() =>
            await _repo.GetAllAsync();

        [HttpGet("{id}")]
        public async Task<ActionResult<Application>> GetById(string id)
        {
            var a = await _repo.GetByIdAsync(id);
            if (a == null) return NotFound();
            return a;
        }

        [HttpPost]
        public async Task<IActionResult> Create([FromBody] Application app)
        {
            await _repo.CreateAsync(app);
            return CreatedAtAction(nameof(GetById), new { id = app.Id }, app);
        }

        [HttpPut("{id}")]
        public async Task<IActionResult> Update(string id, [FromBody] Application app)
        {
            await _repo.UpdateAsync(id, app);
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
