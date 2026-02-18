using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using DeWaveFreeAPI.Models;
using DeWaveFreeAPI.Data;
using DeWaveFreeAPI.DTOs.CourseInstructor;

namespace DeWaveFreeAPI.Controllers;

[Route("api/[controller]")]
[ApiController]
public class CourseInstructorsController : ControllerBase
{
    private readonly DeWaveAPIDbContext _context; // Updated to use actual context name

    public CourseInstructorsController(DeWaveAPIDbContext context)
    {
        _context = context;
    }

    // GET: api/CourseInstructors
    [HttpGet]
    public async Task<ActionResult<IEnumerable<CourseInstructor>>> GetCourseInstructors()
    {
        return await _context.CourseInstructors
            .Include(ci => ci.Instructor)
            .Include(ci => ci.Course)
            .OrderBy(ci => ci.CourseId)
            .ThenBy(ci => ci.SortOrder)
            .ToListAsync();
    }

    // GET: api/CourseInstructors/course/5
    [HttpGet("course/{courseId}")]
    public async Task<ActionResult<IEnumerable<object>>> GetInstructorsByCourse(int courseId)
    {
        var instructors = await _context.CourseInstructors
            .Where(ci => ci.CourseId == courseId)
            .Include(ci => ci.Instructor)
            .OrderBy(ci => ci.SortOrder ?? int.MaxValue)
            .Select(ci => new
            {
                instructorId = ci.InstructorId,
                name = ci.Instructor.Name,
                bio = ci.Instructor.Bio,
                imageUrl = ci.Instructor.ImageUrl,
                contactEmail = ci.Instructor.ContactEmail,
                phoneNumber = ci.Instructor.PhoneNumber,
                certifications = ci.Instructor.Certifications,
                sortOrder = ci.SortOrder
            })
            .ToListAsync();

        if (!instructors.Any())
        {
            return NotFound();
        }

        return Ok(instructors);
    }

    // GET: api/CourseInstructors/instructor/5
    [HttpGet("instructor/{instructorId}")]
    public async Task<ActionResult<IEnumerable<object>>> GetCoursesByInstructor(int instructorId)
    {
        var courses = await _context.CourseInstructors
            .Where(ci => ci.InstructorId == instructorId)
            .Include(ci => ci.Course)
            .Select(ci => new
            {
                courseId = ci.CourseId,
                title = ci.Course.Title,
                description = ci.Course.Description,
                image = ci.Course.Image,
            })
            .ToListAsync();

        if (!courses.Any())
        {
            return NotFound();
        }

        return Ok(courses);
    }

    // POST: api/CourseInstructors
    [HttpPost]
    public async Task<ActionResult<CourseInstructor>> AddCourseInstructor(CourseInstructorCreateDto dto)
    {
        // Check if the relationship already exists
        var exists = await _context.CourseInstructors
            .AnyAsync(ci => ci.CourseId == dto.CourseId &&
                           ci.InstructorId == dto.InstructorId);

        if (exists)
        {
            return Conflict("This instructor is already assigned to this course.");
        }

        // Create a new instance with only the properties we need (avoid navigation property issues)
        var newCourseInstructor = new CourseInstructor
        {
            CourseId = dto.CourseId,
            InstructorId = dto.InstructorId,
            SortOrder = dto.SortOrder
        };

        _context.CourseInstructors.Add(newCourseInstructor);
        await _context.SaveChangesAsync();

        return CreatedAtAction(nameof(GetCourseInstructors), null, newCourseInstructor);
    }

    // DELETE: api/CourseInstructors/course/5/instructor/3
    [HttpDelete("course/{courseId}/instructor/{instructorId}")]
    public async Task<IActionResult> RemoveCourseInstructor(int courseId, int instructorId)
    {
        var courseInstructor = await _context.CourseInstructors
            .FindAsync(courseId, instructorId);

        if (courseInstructor == null)
        {
            return NotFound();
        }

        _context.CourseInstructors.Remove(courseInstructor);
        await _context.SaveChangesAsync();

        return NoContent();
    }
}