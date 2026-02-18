using DeWaveFreeAPI.Data;
using DeWaveFreeAPI.DTOs;
using DeWaveFreeAPI.Extension;
using DeWaveFreeAPI.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System;
using System.Security.Claims;

namespace DeWaveFreeAPI.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class StudentCoursesController : ControllerBase
    {
        private readonly DeWaveAPIDbContext _dbContext;

        public StudentCoursesController(DeWaveAPIDbContext dbContext)
        {
            _dbContext = dbContext;
        }

        [HttpGet("my-courses")]
        [Authorize]
        public async Task<ActionResult<List<CourseDto>>> GetMyCourses()
        {
            var userId = User.GetUserId();
            if (userId == null) return Unauthorized();

            var student = await _dbContext.Students
                .FirstOrDefaultAsync(s => s.UserId == userId.Value);

            if (student == null)
                return NotFound("Student profile not found");

            var courses = await _dbContext.StudentCourses
                .Where(sc => sc.StudentId == student.Id && sc.IsActive)
                .Include(sc => sc.Course)
                .Select(sc => new CourseDto
                {
                    Id = sc.Course.Id,
                    Title = sc.Course.Title,
                    Description = sc.Course.Description,
                    Instructor = sc.Course.Instructor,
                    Duration = sc.Course.Duration,
                    VideoCount = sc.Course.VideoCount,
                    Rating = sc.Course.Rating,
                    Image = sc.Course.Image,
                    CreatedAt = sc.Course.CreatedAt,
                    Price = sc.Course.Price,
                    InstructorId = sc.Course.InstructorId,
                    CategoryId = sc.Course.CategoryId,
                    IsActive = sc.Course.IsActive
                })
                .ToListAsync();

            return Ok(courses);
        }
    }
}
