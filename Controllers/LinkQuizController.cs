using DeWaveFreeAPI.Data;
using DeWaveFreeAPI.DTOs.Lessons;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace DeWaveFreeAPI.Controllers
{
    [ApiController]
    [Route("api/lessons/{lessonId}/quiz-link")]
    public class LinkQuizController : ControllerBase
    {
        private readonly DeWaveAPIDbContext _db;
        public LinkQuizController(DeWaveAPIDbContext db) => _db = db;

        [HttpGet]
        [Authorize]
        public async Task<IActionResult> GetLink(int lessonId)
        {
            var lesson = await _db.Lessons.FirstOrDefaultAsync(l => l.Id == lessonId);
            if (lesson == null) return NotFound();

            if (lesson.SourceLessonId == null)
                return Ok(new { linked = false });

            var source = await _db.Lessons.FirstOrDefaultAsync(l => l.Id == lesson.SourceLessonId);
            return Ok(new { linked = true, sourceLessonId = lesson.SourceLessonId, sourceTitle = source?.Title });
        }

        [HttpPost]
        [Authorize(Roles = "admin,instructor")]
        public async Task<IActionResult> LinkQuiz(int lessonId, [FromBody] LinkQuizDto dto)
        {
            if (lessonId == dto.SourceLessonId)
                return BadRequest("A lesson cannot reuse itself.");

            var lesson = await _db.Lessons.FirstOrDefaultAsync(l => l.Id == lessonId);
            if (lesson == null) return NotFound("Lesson not found");

            var source = await _db.Lessons.FirstOrDefaultAsync(l => l.Id == dto.SourceLessonId);
            if (source == null) return NotFound("Source lesson not found");

            if (source.LessonType != "quiz")
                return BadRequest("Source lesson is not a quiz");

            // don't allow chaining links (A -> B -> C); always point at the canonical quiz
            if (source.SourceLessonId != null)
                return BadRequest("Source lesson is itself a reused quiz; link to the original instead.");

            // avoid silently orphaning content this lesson already owns
            var ownsBlocks = await _db.LessonBlocks.AnyAsync(b => b.LessonId == lessonId);
            if (ownsBlocks)
                return Conflict("This lesson already has its own blocks. Remove them before linking to a reused quiz.");

            lesson.SourceLessonId = dto.SourceLessonId;
            lesson.LessonType = "quiz";
            await _db.SaveChangesAsync();

            return NoContent();
        }

        [HttpDelete]
        [Authorize(Roles = "admin,instructor")]
        public async Task<IActionResult> UnlinkQuiz(int lessonId)
        {
            var lesson = await _db.Lessons.FirstOrDefaultAsync(l => l.Id == lessonId);
            if (lesson == null) return NotFound();

            lesson.SourceLessonId = null;
            await _db.SaveChangesAsync();
            return NoContent();
        }
    }
}
