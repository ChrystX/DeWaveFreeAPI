using DeWaveFreeAPI.Data;
using DeWaveFreeAPI.DTOs.Lessons;
using DeWaveFreeAPI.DTOs.Quiz;
using DeWaveFreeAPI.Extension;
using DeWaveFreeAPI.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;

namespace DeWaveFreeAPI.Controllers
{
    [ApiController]
    [Route("api/sections/{sectionId}/lessons")]
    public class LessonsController : ControllerBase
    {
        private readonly DeWaveAPIDbContext _dbContext;
         
        public LessonsController(DeWaveAPIDbContext dbContext)
        {
            _dbContext = dbContext;
        }

        [HttpGet]
        [Authorize]
        public async Task<ActionResult<IEnumerable<LessonDto>>> GetLessons(int sectionId)
        {
            var lessons = await _dbContext.Lessons
                .Where(l => l.SectionId == sectionId)
                .OrderBy(l => l.SortOrder)
                .Select(l => new LessonDto
                {
                    Id = l.Id,
                    SectionId = l.SectionId,
                    Title = l.Title,
                    Description = l.Description,
                    SortOrder = l.SortOrder,
                    LessonType = l.LessonType,
                    SettingsJson = l.SettingsJson
                })
                .ToListAsync();

            return Ok(lessons);
        }

        [HttpPatch("/api/lessons/{lessonId}/settings")]
        [Authorize(Roles = "admin,instructor")]
        public async Task<IActionResult> UpdateSettings(int lessonId, [FromBody] UpdateExamSettingsDto dto)
        {
            var lesson = await _dbContext.Lessons.FirstOrDefaultAsync(l => l.Id == lessonId);
            if (lesson == null) return NotFound();

            lesson.SettingsJson = dto.SettingsJson;
            await _dbContext.SaveChangesAsync();
            return NoContent();
        }

        [HttpPost]
        [Authorize(Roles = "admin,instructor")]
        public async Task<ActionResult<LessonDto>> CreateLesson(
             int sectionId,
             CreateLessonDto dto)
        {

            var sectionExists = await _dbContext.CourseLearningSections
                .AnyAsync(s => s.Id == sectionId);

            if (!sectionExists)
                return NotFound("Section not found");

            var lesson = new Lesson
            {
                SectionId = sectionId,
                Title = dto.Title,
                Description = dto.Description,
                SortOrder = dto.SortOrder,
                LessonType = dto.LessonType ?? "lesson",
            };

            _dbContext.Lessons.Add(lesson);
            await _dbContext.SaveChangesAsync();

            return Ok(new LessonDto
            {
                Id = lesson.Id,
                SectionId = lesson.SectionId,
                Title = lesson.Title,
                Description = lesson.Description,
                SortOrder = lesson.SortOrder,
                LessonType = lesson.LessonType,
            });
        }

        [HttpGet("/api/lessons/{lessonId}")]
        [Authorize]
        public async Task<ActionResult<LessonDto>> GetLesson(int lessonId)
        {
            var userId = User.GetUserId();
            if (userId == null) return Unauthorized();

            // Resolve the Student record tied to this User
            var student = await _dbContext.Students
                .FirstOrDefaultAsync(s => s.UserId == userId);

            if (student == null) return Unauthorized();

            var lesson = await _dbContext.Lessons
                .FirstOrDefaultAsync(l => l.Id == lessonId);

            if (lesson == null) return NotFound();

            // Find the previous lesson by SortOrder within the same section
            var prevLesson = await _dbContext.Lessons
                .Where(l => l.SectionId == lesson.SectionId && l.SortOrder < lesson.SortOrder)
                .OrderByDescending(l => l.SortOrder)
                .FirstOrDefaultAsync();

            if (prevLesson != null)
            {
                // Completion = a record exists for this student + lesson
                var prevCompleted = await _dbContext.StudentLessonProgresses
                    .AnyAsync(p =>
                        p.StudentId == student.Id &&
                        p.LessonId == prevLesson.Id);

                if (!prevCompleted)
                    return StatusCode(403, new { message = "Complete the previous lesson first." });
            }

            return Ok(new LessonDto
            {
                Id = lesson.Id,
                SectionId = lesson.SectionId,
                Title = lesson.Title,
                Description = lesson.Description,
                SortOrder = lesson.SortOrder,
                LessonType = lesson.LessonType,
                SettingsJson = lesson.SettingsJson
            });
        }

        [HttpPut("{lessonId}")]
        [Authorize(Roles = "admin,instructor")]
        public async Task<IActionResult> UpdateLesson(
            int sectionId, int lessonId, UpdateLessonDto dto)
        {
            var lesson = await _dbContext.Lessons
                .FirstOrDefaultAsync(l => l.Id == lessonId && l.SectionId == sectionId);

            if (lesson == null)
                return NotFound();

            lesson.Title = dto.Title;
            lesson.Description = dto.Description;
            lesson.SortOrder = dto.SortOrder;

            await _dbContext.SaveChangesAsync();

            return NoContent();
        }

        [HttpDelete("{lessonId}")]
        [Authorize(Roles = "admin,instructor")]
        public async Task<IActionResult> DeleteLesson(int sectionId, int lessonId)
        {
            var lesson = await _dbContext.Lessons
                .FirstOrDefaultAsync(l => l.Id == lessonId && l.SectionId == sectionId);

            if (lesson == null)
                return NotFound();

            _dbContext.Lessons.Remove(lesson);
            await _dbContext.SaveChangesAsync();

            return NoContent();
        }
    }
}
