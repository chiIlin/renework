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

        public BusinessDataController(IBusinessDataRepository repo)
        {
            _repo = repo;
        }

        /// <summary>
        /// Get the business data for a given user ID (any authenticated user).
        /// </summary>
        [HttpGet("{userId}")]
        [Authorize]
        public async Task<ActionResult<BusinessData>> GetByUserId(string userId)
        {
            var data = await _repo.GetByUserIdAsync(userId);
            if (data == null) return NotFound();
            return Ok(data);
        }

        /// <summary>
        /// Get all business data (admin only).
        /// </summary>
        [HttpGet]
        [Authorize(Policy = "AdminOnly")]
        public async Task<ActionResult<List<BusinessData>>> GetAll()
        {
            var all = await _repo.GetAllAsync();
            return Ok(all);
        }

        /// <summary>
        /// Create or initialize this user's business data (business or admin).
        /// </summary>
        [HttpPost]
        [Authorize(Roles = "Business,Admin")]
        public async Task<IActionResult> Create([FromBody] BusinessDataDto dto)
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier)!;
            if (await _repo.GetByUserIdAsync(userId) != null)
                return BadRequest("Business data already exists for this user.");

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
                DowntimeMonths = dto.DowntimeMonths,
                TotalLosses = 0, // stub, under development
                CreatedAt = DateTime.UtcNow
            };

            await _repo.CreateAsync(data);
            return CreatedAtAction(
                nameof(GetByUserId),
                new { userId = data.UserId },
                data
            );
        }

        /// <summary>
        /// Update existing business data by its document ID (business or admin).
        /// </summary>
        [HttpPut("{id}")]
        [Authorize(Roles = "Business,Admin")]
        public async Task<IActionResult> Update(string id, [FromBody] BusinessDataDto dto)
        {
            var existing = await _repo.GetByUserIdAsync(User.FindFirstValue(ClaimTypes.NameIdentifier)!);
            if (existing == null || existing.Id != id)
                return NotFound();

            existing.Location.City = dto.City;
            existing.Location.Region = dto.Region;
            existing.Location.Address = dto.Address;
            existing.AreaSqm = dto.AreaSqm;
            existing.MonthlyRevenue = dto.MonthlyRevenue;
            existing.Budget = dto.Budget;
            existing.Description = dto.Description;
            existing.DowntimeMonths = dto.DowntimeMonths;
            // existing.TotalLosses left unchanged

            await _repo.UpdateAsync(id, existing);
            return NoContent();
        }

        /// <summary>
        /// Delete business data by user ID (admin only).
        /// </summary>
        [HttpDelete("{userId}")]
        [Authorize(Policy = "AdminOnly")]
        public async Task<IActionResult> Delete(string userId)
        {
            await _repo.DeleteByUserIdAsync(userId);
            return NoContent();
        }
    }
}
