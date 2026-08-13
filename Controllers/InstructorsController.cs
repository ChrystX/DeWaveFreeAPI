using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using DeWaveFreeAPI.Models;
using DeWaveFreeAPI.Data;
using DeWaveFreeAPI.DTOs;

namespace DeWaveFreeAPI.Controllers
{

    [Route("api/instructors")]
    [ApiController]
    public class InstructorsController : ControllerBase
    {
        private readonly DeWaveAPIDbContext _dbContext;

        public InstructorsController(DeWaveAPIDbContext dbContext)
        {
            _dbContext = dbContext;
        }

        private static InstructorDto MapToDto(Instructor i) => new InstructorDto
        {
            Id = i.Id,
            Name = i.Name,
            Bio = i.Bio,
            ImageUrl = i.ImageUrl,
            ContactEmail = i.ContactEmail,
            PhoneNumber = i.PhoneNumber,
            Certifications = i.Certifications,
            Headline = i.Headline,
            Specialization = i.Specialization,
            CreatedAt = i.CreatedAt,
            UpdatedAt = i.UpdatedAt,
        };

        [HttpGet]
        public async Task<ActionResult<IEnumerable<InstructorDto>>> GetInstructors()
        {
            var instructors = await _dbContext.Instructors.ToListAsync();
            return Ok(instructors.Select(MapToDto).ToList());
        }

        [HttpGet("{id:int}")]
        public async Task<ActionResult<InstructorDto>> GetInstructor(int id)
        {
            var instructor = await _dbContext.Instructors.FindAsync(id);
            if (instructor == null) return NotFound();
            return Ok(MapToDto(instructor));
        }

        [HttpPost]
        [Authorize(Roles = "admin")]
        public async Task<ActionResult<InstructorDto>> PostInstructor([FromBody] InstructorCreateDto dto)
        {
            var instructor = new Instructor
            {
                Name = dto.Name,
                Bio = dto.Bio,
                ImageUrl = dto.ImageUrl,
                ContactEmail = dto.ContactEmail,
                PhoneNumber = dto.PhoneNumber,
                Certifications = dto.Certifications,
                Headline = dto.Headline,
                Specialization = dto.Specialization,
                CreatedAt = DateTime.UtcNow,
            };

            _dbContext.Instructors.Add(instructor);
            await _dbContext.SaveChangesAsync();

            return CreatedAtAction(nameof(GetInstructor), new { id = instructor.Id }, MapToDto(instructor));
        }

        [HttpGet("by-user/{userId}")]
        public async Task<ActionResult<InstructorDto>> GetInstructorByUserId(int userId)
        {
            var instructor = await _dbContext.Instructors.FirstOrDefaultAsync(i => i.UserId == userId);
            if (instructor == null) return NotFound();
            return Ok(MapToDto(instructor));
        }

        [HttpPut("{id}")]
        [Authorize(Roles = "admin")]
        public async Task<IActionResult> PutInstructor(int id, [FromBody] InstructorCreateDto dto)
        {
            var instructor = await _dbContext.Instructors.FindAsync(id);
            if (instructor == null) return NotFound();

            instructor.Name = dto.Name;
            instructor.Bio = dto.Bio;
            instructor.ImageUrl = dto.ImageUrl;
            instructor.ContactEmail = dto.ContactEmail;
            instructor.PhoneNumber = dto.PhoneNumber;
            instructor.Certifications = dto.Certifications;
            instructor.Headline = dto.Headline;
            instructor.Specialization = dto.Specialization;
            instructor.UpdatedAt = DateTime.UtcNow;

            await _dbContext.SaveChangesAsync();

            return NoContent();
        }

        [HttpDelete("{id}")]
        [Authorize(Roles = "admin")]
        public async Task<IActionResult> DeleteInstructor(int id)
        {
            var instructor = await _dbContext.Instructors.FindAsync(id);
            if (instructor == null) return NotFound();

            _dbContext.Instructors.Remove(instructor);
            await _dbContext.SaveChangesAsync();

            return NoContent();
        }
    }
}
