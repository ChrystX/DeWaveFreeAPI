using DeWaveFreeAPI.Data;
using DeWaveFreeAPI.DTOs;
using DeWaveFreeAPI.Extension;
using DeWaveFreeAPI.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;

namespace DeWaveFreeAPI.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize]
    public class StudentProgressController : ControllerBase
    {
        private readonly DeWaveAPIDbContext _dbContext;

        public StudentProgressController(DeWaveAPIDbContext dbContext)
        {
            _dbContext = dbContext;
        }

        // POST /api/student-progress/complete
        [HttpPost("complete")]
        public async Task<IActionResult> MarkLessonComplete([FromBody] MarkCompleteDto dto)
        {
            var student = await GetStudentAsync();
            if (student == null) return NotFound("Student profile not found");

            var lessonExists = await _dbContext.Lessons.AnyAsync(l => l.Id == dto.LessonId);
            if (!lessonExists) return NotFound("Lesson not found");

            var alreadyExists = await _dbContext.StudentLessonProgresses
                .AnyAsync(p => p.StudentId == student.Id && p.LessonId == dto.LessonId);

            if (!alreadyExists)
            {
                _dbContext.StudentLessonProgresses.Add(new StudentLessonProgress
                {
                    StudentId = student.Id,
                    LessonId = dto.LessonId,
                    CompletedAt = DateTime.UtcNow,
                    Status = "completed",
                });
                await _dbContext.SaveChangesAsync();
            }

            return Ok();
        }

        // DELETE /api/student-progress/uncomplete
        [HttpDelete("uncomplete")]
        public async Task<IActionResult> UnmarkLessonComplete([FromBody] MarkCompleteDto dto)
        {
            var student = await GetStudentAsync();
            if (student == null) return NotFound("Student profile not found");

            var record = await _dbContext.StudentLessonProgresses
                .FirstOrDefaultAsync(p => p.StudentId == student.Id && p.LessonId == dto.LessonId);

            if (record != null)
            {
                _dbContext.StudentLessonProgresses.Remove(record);
                await _dbContext.SaveChangesAsync();
            }

            return Ok();
        }

        [HttpGet("course/{courseId}/lessons")]
        public async Task<ActionResult<List<int>>> GetCompletedLessonIds(int courseId)
        {
            var student = await GetStudentAsync();
            if (student == null) return NotFound("Student profile not found");

            var ids = await _dbContext.StudentLessonProgresses
                .Where(p => p.StudentId == student.Id && p.Lesson.Section.CourseId == courseId)
                .Select(p => p.LessonId)
                .ToListAsync();

            return Ok(ids);
        }

        // GET /api/student-progress/course/{courseId}
        [HttpGet("course/{courseId}")]
        public async Task<ActionResult<CourseProgressDto>> GetCourseProgress(int courseId)
        {
            var student = await GetStudentAsync();
            if (student == null) return NotFound("Student profile not found");

            var totalLessons = await _dbContext.Lessons
                .Where(l => l.Section.CourseId == courseId)
                .CountAsync();

            if (totalLessons == 0)
                return Ok(new CourseProgressDto { CourseId = courseId, CompletedLessons = 0, TotalLessons = 0, Percentage = 0 });

            var completedLessons = await _dbContext.StudentLessonProgresses
                .Where(p => p.StudentId == student.Id && p.Lesson.Section.CourseId == courseId)
                .CountAsync();

            var percentage = (int)Math.Round((double)completedLessons / totalLessons * 100);

            return Ok(new CourseProgressDto
            {
                CourseId = courseId,
                CompletedLessons = completedLessons,
                TotalLessons = totalLessons,
                Percentage = percentage
            });
        }

        // GET /api/student-progress/courses
        [HttpGet("courses")]
        public async Task<ActionResult<List<CourseProgressDto>>> GetAllCoursesProgress()
        {
            var student = await GetStudentAsync();
            if (student == null) return NotFound("Student profile not found");

            var enrolledCourseIds = await _dbContext.StudentCourses
                .Where(sc => sc.StudentId == student.Id && sc.IsActive)
                .Select(sc => sc.CourseId)
                .ToListAsync();

            var result = new List<CourseProgressDto>();

            foreach (var courseId in enrolledCourseIds)
            {
                var totalLessons = await _dbContext.Lessons
                    .Where(l => l.Section.CourseId == courseId)
                    .CountAsync();

                var completedLessons = totalLessons == 0 ? 0 : await _dbContext.StudentLessonProgresses
                    .Where(p => p.StudentId == student.Id && p.Lesson.Section.CourseId == courseId)
                    .CountAsync();

                var percentage = totalLessons == 0 ? 0 : (int)Math.Round((double)completedLessons / totalLessons * 100);

                result.Add(new CourseProgressDto
                {
                    CourseId = courseId,
                    CompletedLessons = completedLessons,
                    TotalLessons = totalLessons,
                    Percentage = percentage
                });
            }

            return Ok(result);
        }

        // GET /api/student-progress/course/{courseId}/resume
        [HttpGet("course/{courseId}/resume")]
        public async Task<ActionResult<int>> GetResumeLessonId(int courseId)
        {
            var student = await GetStudentAsync();
            if (student == null) return NotFound("Student profile not found");

            var allLessons = await _dbContext.Lessons
                .Where(l => l.Section.CourseId == courseId)
                .OrderBy(l => l.Section.SortOrder)
                .ThenBy(l => l.SortOrder)
                .Select(l => l.Id)
                .ToListAsync();

            if (!allLessons.Any()) return NotFound("No lessons found for this course");

            var completedIds = await _dbContext.StudentLessonProgresses
                .Where(p => p.StudentId == student.Id && p.Lesson.Section.CourseId == courseId)
                .Select(p => p.LessonId)
                .ToListAsync();

            var resumeId = allLessons.FirstOrDefault(id => !completedIds.Contains(id));

            // All done — return last lesson for review
            return Ok(resumeId == 0 ? allLessons.Last() : resumeId);
        }


        private async Task<Student?> GetStudentAsync()
        {
            var userId = User.GetUserId();
            if (userId == null) return null;

            return await _dbContext.Students
                .FirstOrDefaultAsync(s => s.UserId == userId.Value);
        }
    }
}