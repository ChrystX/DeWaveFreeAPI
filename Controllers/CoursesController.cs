using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using DeWaveFreeAPI.Models;
using DeWaveFreeAPI.Data;

namespace DeWaveFreeAPI.Controllers
{
    public class CourseDto
    {
        public int Id { get; set; }
        public string Title { get; set; } = null!;
        public string? Description { get; set; }
        public string? Instructor { get; set; }
        public int? Duration { get; set; }
        public int? VideoCount { get; set; }
        public decimal? Rating { get; set; }
        public string? Image { get; set; }
        public DateTime? CreatedAt { get; set; }
        public int InstructorId { get; set; }
        public int? CategoryId { get; set; }
        public bool? IsActive { get; set; }
    }

    [Route("api/courses")]
    [ApiController]
    public class CoursesController : ControllerBase
    {
        private readonly DeWaveAPIDbContext _dbContext;

        public CoursesController(DeWaveAPIDbContext context)
        {
            _dbContext = context;
        }

        [HttpGet]
        public async Task<ActionResult<IEnumerable<CourseDto>>> GetCourses()
        {
            var courses = await _dbContext.Courses.ToListAsync();

            var courseDtos = courses.Select(course => new CourseDto
            {
                Id = course.Id,
                Title = course.Title,
                Description = course.Description,
                Instructor = course.Instructor,
                Duration = course.Duration,
                VideoCount = course.VideoCount,
                Rating = course.Rating,
                Image = course.Image,
                CreatedAt = course.CreatedAt,
                InstructorId = course.InstructorId,
                CategoryId = course.CategoryId,
                IsActive = course.IsActive
            }).ToList();

            return courseDtos;
        }

        [HttpGet("active")]
        public async Task<ActionResult<IEnumerable<CourseDto>>> GetActiveCourses()
        {
            var courses = await _dbContext.Courses
                .Where(c => c.IsActive == true)
                .ToListAsync();

            var courseDtos = courses.Select(course => new CourseDto
            {
                Id = course.Id,
                Title = course.Title,
                Description = course.Description,
                Instructor = course.Instructor,
                Duration = course.Duration,
                VideoCount = course.VideoCount,
                Rating = course.Rating,
                Image = course.Image,
                CreatedAt = course.CreatedAt,
                InstructorId = course.InstructorId,
                CategoryId = course.CategoryId,
                IsActive = course.IsActive
            }).ToList();

            return courseDtos;
        }

        [HttpGet("{id}")]
        public async Task<ActionResult<CourseDto>> GetCourse(int id)
        {
            var course = await _dbContext.Courses.FindAsync(id);

            if (course == null) return NotFound();

            var dto = new CourseDto
            {
                Id = course.Id,
                Title = course.Title,
                Description = course.Description,
                Instructor = course.Instructor,
                Duration = course.Duration,
                VideoCount = course.VideoCount,
                Rating = course.Rating,
                Image = course.Image,
                CreatedAt = course.CreatedAt,
                InstructorId = course.InstructorId,
                CategoryId = course.CategoryId,
                IsActive = course.IsActive
            };

            return dto;
        }


        [HttpPost("bulk")]
        public async Task<IActionResult> PostCoursesBulk([FromBody] IEnumerable<CourseDto> courseDtos)
        {
            var courses = courseDtos.Select(dto => new Course
            {
                Title = dto.Title,
                Description = dto.Description,
                Instructor = dto.Instructor,
                Duration = dto.Duration,
                VideoCount = dto.VideoCount,
                Rating = dto.Rating,
                Image = dto.Image,
                CreatedAt = dto.CreatedAt ?? DateTime.Now,
                InstructorId = dto.InstructorId,
                CategoryId = dto.CategoryId,
                IsActive = dto.IsActive ?? true
            }).ToList();

            _dbContext.Courses.AddRange(courses);
            await _dbContext.SaveChangesAsync();

            return Ok($"Successfully created {courses.Count} courses");
        }

        [HttpPost]
        public async Task<IActionResult> PostCourse([FromBody] CourseDto dto)
        {
            var course = new Course
            {
                Title = dto.Title,
                Description = dto.Description,
                Instructor = dto.Instructor,
                Duration = dto.Duration,
                VideoCount = dto.VideoCount,
                Rating = dto.Rating,
                Image = dto.Image,
                CreatedAt = dto.CreatedAt ?? DateTime.Now,
                InstructorId = dto.InstructorId,
                CategoryId = dto.CategoryId,
                IsActive = dto.IsActive ?? true
            };

            _dbContext.Courses.Add(course);
            await _dbContext.SaveChangesAsync();

            return CreatedAtAction(nameof(GetCourse), new { id = course.Id }, course);
        }

        [HttpPut("{id}")]
        public async Task<IActionResult> PutCourse(int id, [FromBody] CourseDto dto)
        {
            if (id != dto.Id) return BadRequest();

            var course = await _dbContext.Courses.FindAsync(id);
            if (course == null) return NotFound();

            course.Title = dto.Title;
            course.Description = dto.Description;
            course.Instructor = dto.Instructor;
            course.Duration = dto.Duration;
            course.VideoCount = dto.VideoCount;
            course.Rating = dto.Rating;
            course.Image = dto.Image;
            course.CreatedAt = dto.CreatedAt;
            course.InstructorId = dto.InstructorId;
            course.CategoryId = dto.CategoryId;
            course.IsActive = dto.IsActive ?? true;

            await _dbContext.SaveChangesAsync();

            return NoContent();
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteCourse(int id)
        {
            var course = await _dbContext.Courses.FindAsync(id);
            if (course == null) return NotFound();

            _dbContext.Courses.Remove(course);
            await _dbContext.SaveChangesAsync();

            return NoContent();
        }
    }
}
