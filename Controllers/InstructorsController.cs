using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using DeWaveFreeAPI.Models;
using DeWaveFreeAPI.Data;

namespace DeWaveFreeAPI.Controllers
{
    public class InstructorDto
    {
        public int Id { get; set; }
        public string Name { get; set; } = null!;
        public string? Bio { get; set; }
        public string? ImageUrl { get; set; }
        public string? ContactEmail { get; set; }
        public string? PhoneNumber { get; set; }
        public string? Certifications { get; set; }
    }

    public class InstructorCreateDto
    {
        public string Name { get; set; } = null!;
        public string? Bio { get; set; }
        public string? ImageUrl { get; set; }
        public string? ContactEmail { get; set; }
        public string? PhoneNumber { get; set; }
        public string? Certifications { get; set; }
    }

    [Route("api/instructors")]
    [ApiController]
    public class InstructorsController : ControllerBase
    {
        private readonly DeWaveAPIDbContext _dbContext;

        public InstructorsController(DeWaveAPIDbContext dbContext)
        {
            _dbContext = dbContext;
        }

        [HttpGet]
        public async Task<ActionResult<IEnumerable<InstructorDto>>> GetInstructors()
        {
            var instructors = await _dbContext.Instructors.ToListAsync();

            var instructorsDto = instructors.Select(i => new InstructorDto
            {
                Id = i.Id,
                Name = i.Name,
                Bio = i.Bio,
                ImageUrl = i.ImageUrl,
                ContactEmail = i.ContactEmail,
                PhoneNumber = i.PhoneNumber,
                Certifications = i.Certifications,
            }).ToList();

            return Ok(instructorsDto);
        }

        [HttpGet("{id}")]
        public async Task<ActionResult<InstructorDto>> GetInstructor(int id)
        {
            var instructor = await _dbContext.Instructors.FindAsync(id);

            if (instructor == null)
            {
                return NotFound();
            }

            var dto = new InstructorDto
            {
                Id = instructor.Id,
                Name = instructor.Name,
                Bio = instructor.Bio,
                ImageUrl = instructor.ImageUrl,
                ContactEmail = instructor.ContactEmail,
                PhoneNumber = instructor.PhoneNumber,
                Certifications = instructor.Certifications,
            };

            return Ok(dto);
        }

        [HttpPost]
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
            };

            _dbContext.Instructors.Add(instructor);
            await _dbContext.SaveChangesAsync();

            var resultDto = new InstructorDto
            {
                Id = instructor.Id,
                Name = instructor.Name,
                Bio = instructor.Bio,
                ImageUrl = instructor.ImageUrl,
                ContactEmail = instructor.ContactEmail,
                PhoneNumber = instructor.PhoneNumber,
                Certifications = instructor.Certifications,
            };

            return CreatedAtAction(nameof(GetInstructor), new { id = instructor.Id }, resultDto);
        }

        [HttpPut("{id}")]
        public async Task<IActionResult> PutInstructor(int id, [FromBody] InstructorCreateDto dto)
        {
            var instructor = await _dbContext.Instructors.FindAsync(id);
            if (instructor == null)
                return NotFound();

            instructor.Name = dto.Name;
            instructor.Bio = dto.Bio;
            instructor.ImageUrl = dto.ImageUrl;
            instructor.ContactEmail = dto.ContactEmail;
            instructor.PhoneNumber = dto.PhoneNumber;
            instructor.Certifications = dto.Certifications;

            await _dbContext.SaveChangesAsync();

            return NoContent();
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteInstructor(int id)
        {
            var instructor = await _dbContext.Instructors.FindAsync(id);
            if (instructor == null)
                return NotFound();

            _dbContext.Instructors.Remove(instructor);
            await _dbContext.SaveChangesAsync();

            return NoContent();
        }
    }
}
