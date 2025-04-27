// Controllers/BusinessDataController.cs
using System;
using System.Collections.Generic;
using System.Security.Claims;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using renework.Dto;
using renework.MongoDB.Collections;
using renework.Repositories.Interfaces;

namespace renework.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class BusinessDataController : ControllerBase
    {
        private readonly IBusinessDataRepository _repo;
        private const double MeterPrice = 50.0;

        public BusinessDataController(IBusinessDataRepository repo)
        {
            _repo = repo;
        }

        [HttpPost]
        [Authorize(Roles = "Business,Admin")]
        public async Task<IActionResult> Create([FromBody] BusinessDataDto dto)
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier)!;
            if (await _repo.GetByUserIdAsync(userId) != null)
                return BadRequest("Already exists");

            var data = new BusinessData
            {
                UserId = userId,
                Location = new BusinessLocation
                {
                    City = dto.City,
                    Region = dto.Region,
                    Address = dto.Address
                },
                AreaSqm = dto.AreaSqm,
                MonthlyRevenue = dto.MonthlyRevenue,
                Budget = dto.Budget,
                Description = dto.Description,
                DowntimeStart = dto.DowntimeStart,
                CreatedAt = DateTime.UtcNow
            };
            data.CalculateTotalLosses(data.DowntimeStart, MeterPrice);
            await _repo.CreateAsync(data);
            return CreatedAtAction(nameof(GetByUserId), new { userId }, data);
        }

        [HttpPut("{id}")]
        [Authorize(Roles = "Business,Admin")]
        public async Task<IActionResult> Update(string id, [FromBody] BusinessDataDto dto)
        {
            var existing = await _repo.GetByUserIdAsync(User.FindFirstValue(ClaimTypes.NameIdentifier)!);
            if (existing == null || existing.Id != id) return NotFound();

            existing.Location.City = dto.City;
            existing.Location.Region = dto.Region;
            existing.Location.Address = dto.Address;
            existing.AreaSqm = dto.AreaSqm;
            existing.MonthlyRevenue = dto.MonthlyRevenue;
            existing.Budget = dto.Budget;
            existing.Description = dto.Description;
            existing.DowntimeStart = dto.DowntimeStart;
            existing.CalculateTotalLosses(existing.DowntimeStart, MeterPrice);

            await _repo.UpdateAsync(id, existing);
            return NoContent();
        }

        [HttpGet("{userId}")]
        [Authorize]
        public async Task<ActionResult<BusinessData>> GetByUserId(string userId)
        {
            var data = await _repo.GetByUserIdAsync(userId);
            if (data == null) return NotFound();
            return Ok(data);
        }

        [HttpGet]
        [Authorize(Policy = "AdminOnly")]
        public async Task<ActionResult<List<BusinessData>>> GetAll()
        {
            return Ok(await _repo.GetAllAsync());
        }

        [HttpDelete("{userId}")]
        [Authorize(Policy = "AdminOnly")]
        public async Task<IActionResult> Delete(string userId)
        {
            await _repo.DeleteByUserIdAsync(userId);
            return NoContent();
        }
    }
}
