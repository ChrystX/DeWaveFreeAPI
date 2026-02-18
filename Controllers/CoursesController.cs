using DeWaveFreeAPI.Data;
using DeWaveFreeAPI.DTOs;
using DeWaveFreeAPI.DTOs.CourseInstructor;
using DeWaveFreeAPI.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace DeWaveFreeAPI.Controllers
{

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
        [AllowAnonymous]
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
                Price = course.Price,
                InstructorId = course.InstructorId,
                CategoryId = course.CategoryId,
                IsActive = course.IsActive
            }).ToList();

            return courseDtos;
        }

        [HttpGet("instructor/{instructorId}")]
        public async Task<ActionResult<IEnumerable<InstructorCourseDto>>> GetCoursesByInstructor(int instructorId)
        {
            var courses = await _dbContext.CourseInstructors
                .Where(ci => ci.InstructorId == instructorId)
                .Include(ci => ci.Course)
                .Select(ci => new InstructorCourseDto
                {
                    CourseId = ci.CourseId,
                    Title = ci.Course.Title,
                    Description = ci.Course.Description,
                    Image = ci.Course.Image,
                })
                .ToListAsync();

            if (!courses.Any()) return NotFound();

            return Ok(courses);
        }

        [HttpGet("active")]
        [AllowAnonymous]
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
                Price = course.Price,
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
                Price = course.Price,
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
                Price = dto.Price ?? 0m,
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
                Price = dto.Price ?? 0m,
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
            course.Price = dto.Price ?? 0m;
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
