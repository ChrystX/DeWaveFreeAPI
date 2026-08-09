using DeWaveFreeAPI.DTOs.Events;
using DeWaveFreeAPI.Extension;
using DeWaveFreeAPI.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace DeWaveFreeAPI.Controllers
{
    [ApiController]
    [Route("api/events")]
    [Authorize]
    public class EventController : ControllerBase
    {
        private readonly IEventCrudService _crudService;

        public EventController(IEventCrudService crudService)
        {
            _crudService = crudService;
        }

        [HttpPost]
        [Authorize(Roles = "admin,instructor")]
        public async Task<IActionResult> CreateEvent([FromBody] CreateEventDto dto)
        {
            var userId = User.GetUserId();
            if (userId == null) return Unauthorized();

            try
            {
                var eventId = await _crudService.CreateEventAsync(dto, userId.Value);
                return CreatedAtAction(nameof(CreateEvent), new { id = eventId }, new { id = eventId });
            }
            catch (ArgumentException ex) { return BadRequest(new { message = ex.Message }); }
            catch (UnauthorizedAccessException ex) { return Forbid(); }
        }

        [HttpPut("{id}")]
        [Authorize(Roles = "admin,instructor")]
        public async Task<IActionResult> UpdateEvent(int id, [FromBody] UpdateEventDto dto)
        {
            var userId = User.GetUserId();
            if (userId == null) return Unauthorized();

            try
            {
                var updated = await _crudService.UpdateEventAsync(id, dto, userId.Value);
                return updated ? NoContent() : NotFound();
            }
            catch (ArgumentException ex) { return BadRequest(new { message = ex.Message }); }
            catch (UnauthorizedAccessException ex) { return Forbid(); }
        }

        [HttpDelete("{id}")]
        [Authorize(Roles = "admin,instructor")]
        public async Task<IActionResult> DeleteEvent(int id)
        {
            var userId = User.GetUserId();
            if (userId == null) return Unauthorized();

            try
            {
                var deleted = await _crudService.DeleteEventAsync(id, userId.Value);
                return deleted ? NoContent() : NotFound();
            }
            catch (UnauthorizedAccessException) { return Forbid(); }
        }

        [HttpPatch("{id}/status")]
        [Authorize(Roles = "admin,instructor")]
        public async Task<IActionResult> ToggleEventStatus(int id, [FromBody] bool isActive)
        {
            var userId = User.GetUserId();
            if (userId == null) return Unauthorized();

            try
            {
                var toggled = await _crudService.ToggleEventStatusAsync(id, userId.Value, isActive);
                return toggled ? NoContent() : NotFound();
            }
            catch (UnauthorizedAccessException) { return Forbid(); }
        }
    }
}