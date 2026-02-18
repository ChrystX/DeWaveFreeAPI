using DeWaveFreeAPI.Data;
using DeWaveFreeAPI.DTOs.Quiz;
using DeWaveFreeAPI.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;
using System.Text.Json;

namespace DeWaveFreeAPI.Controllers
{
    [ApiController]
    [Route("api/quiz")]
    public class QuizController : ControllerBase
    {
        private readonly DeWaveAPIDbContext _db;
        public QuizController(DeWaveAPIDbContext db) => _db = db;

        [HttpPost("start")]
        [Authorize]
        public async Task<IActionResult> Start([FromBody] StartQuizDto dto)
        {
            var userId = int.Parse(User.FindFirst(ClaimTypes.NameIdentifier)!.Value);

            var existing = await _db.QuizAttempts
                .Where(a => a.UserId == userId && a.LessonId == dto.LessonId && a.Status == "in_progress")
                .FirstOrDefaultAsync();

            if (existing != null)
                return Ok(new { attemptId = existing.Id, resumed = true });

            var attemptNumber = await _db.QuizAttempts
                .Where(a => a.UserId == userId && a.LessonId == dto.LessonId)
                .CountAsync() + 1;

            var attempt = new QuizAttempt
            {
                UserId = userId,
                LessonId = dto.LessonId,
                AttemptNumber = attemptNumber,
                Status = "in_progress",
                StartedAt = DateTime.UtcNow
            };

            _db.QuizAttempts.Add(attempt);
            await _db.SaveChangesAsync();
            return Ok(new { attemptId = attempt.Id, resumed = false });
        }

        [HttpPost("submit")]
        [Authorize]
        public async Task<IActionResult> Submit([FromBody] SubmitQuizDto dto)
        {
            var userId = int.Parse(User.FindFirst(ClaimTypes.NameIdentifier)!.Value);

            var attempt = await _db.QuizAttempts
                .FirstOrDefaultAsync(a => a.Id == dto.AttemptId && a.UserId == userId);

            if (attempt == null) return NotFound();
            if (attempt.Status == "submitted") return BadRequest("Already submitted");

            // load the lesson to get settings_json
            var lesson = await _db.Lessons
                .FirstOrDefaultAsync(l => l.Id == attempt.LessonId);

            if (lesson == null) return NotFound("Lesson not found");

            var examSettings = JsonSerializer.Deserialize<ExamSettingsJson>(lesson.SettingsJson!);

            // load all question blocks for this lesson
            var questionBlocks = await _db.LessonBlocks
                .Where(b => b.LessonId == attempt.LessonId && b.BlockTypeId == 7)
                .OrderBy(b => b.OrderIndex)
                .ToListAsync();

            var totalWeight = questionBlocks.Sum(b =>
            {
                var q = JsonSerializer.Deserialize<QuestionJson>(b.DataJson!);
                return q?.Weight ?? 1.0;
            });

            decimal earnedScore = 0;
            var answerRows = new List<QuizAttemptAnswer>();

            foreach (var qBlock in questionBlocks)
            {
                var q = JsonSerializer.Deserialize<QuestionJson>(qBlock.DataJson!);
                dto.Answers.TryGetValue(qBlock.Id.ToString(), out var userAnswer);

                var correct = ScoreQuestion(q!, userAnswer);
                var weight = q!.Weight ?? 1.0;
                var normalizedPoints = (decimal)(weight / totalWeight) * 100;

                if (correct) earnedScore += normalizedPoints;

                answerRows.Add(new QuizAttemptAnswer
                {
                    AttemptId = attempt.Id,
                    QuestionId = qBlock.Id,
                    QuestionType = q.Type,
                    UserAnswer = userAnswer,
                    IsCorrect = correct,
                    PointsEarned = correct ? (int)Math.Round(normalizedPoints) : 0,
                    PointsPossible = (int)Math.Round(normalizedPoints)
                });
            }

            var score = Math.Round(earnedScore, 2);
            attempt.AnswersJson = JsonSerializer.Serialize(dto.Answers);
            attempt.Score = score;
            attempt.Passed = score >= (examSettings?.PassingScore ?? 70);
            attempt.Status = "submitted";
            attempt.SubmittedAt = DateTime.UtcNow;

            _db.QuizAttemptAnswers.AddRange(answerRows);

            if (attempt.Passed == true)
            {
                var progress = await _db.StudentLessonProgresses
                    .FirstOrDefaultAsync(p => p.StudentId == userId && p.LessonId == attempt.LessonId);

                if (progress == null)
                {
                    progress = new StudentLessonProgress
                    {
                        StudentId = userId,
                        LessonId = attempt.LessonId ?? throw new InvalidOperationException("Attempt missing LessonId"),
                        StartedAt = attempt.StartedAt
                    };
                    _db.StudentLessonProgresses.Add(progress);
                }

                progress.Status = "completed";
                progress.Score = score;
                progress.CompletedAt = DateTime.UtcNow;
                progress.LastAccessedAt = DateTime.UtcNow;
            }

            await _db.SaveChangesAsync();

            return Ok(new
            {
                score,
                passed = attempt.Passed,
                totalQuestions = questionBlocks.Count,
                answeredCorrect = answerRows.Count(a => a.IsCorrect == true),
                passingScore = examSettings?.PassingScore ?? 70
            });
        }

        [HttpPost("abandon")]
        [Authorize]
        public async Task<IActionResult> Abandon([FromBody] AbandonQuizDto dto)
        {
            var userId = int.Parse(User.FindFirst(ClaimTypes.NameIdentifier)!.Value);
            var attempt = await _db.QuizAttempts
                .FirstOrDefaultAsync(a => a.Id == dto.AttemptId && a.UserId == userId);

            if (attempt == null) return NotFound();
            if (attempt.Status != "in_progress") return BadRequest();

            attempt.Status = "abandoned";
            await _db.SaveChangesAsync();
            return NoContent();
        }

        [HttpGet("{attemptId}")]
        [Authorize]
        public async Task<IActionResult> GetResult(int attemptId)
        {
            var userId = int.Parse(User.FindFirst(ClaimTypes.NameIdentifier)!.Value);

            var attempt = await _db.QuizAttempts
                .Include(a => a.QuizAttemptAnswers)
                .FirstOrDefaultAsync(a => a.Id == attemptId && a.UserId == userId);

            if (attempt == null) return NotFound();

            return Ok(new
            {
                attempt.Score,
                attempt.Passed,
                attempt.Status,
                attempt.SubmittedAt,
                breakdown = attempt.QuizAttemptAnswers.Select(a => new
                {
                    a.QuestionId,
                    a.QuestionType,
                    a.UserAnswer,
                    a.IsCorrect,
                    a.PointsEarned,
                    a.PointsPossible
                })
            });
        }

        [HttpGet("lesson/{lessonId}/latest")]
        [Authorize]
        public async Task<IActionResult> GetLatestAttempt(int lessonId)
        {
            var userId = int.Parse(User.FindFirst(ClaimTypes.NameIdentifier)!.Value);

            var attempt = await _db.QuizAttempts
                .Include(a => a.QuizAttemptAnswers)
                .Where(a => a.LessonId == lessonId && a.UserId == userId)
                .OrderByDescending(a => a.AttemptNumber)
                .FirstOrDefaultAsync();

            if (attempt == null) return NotFound();

            var lesson = await _db.Lessons.FirstOrDefaultAsync(l => l.Id == lessonId);
            var examSettings = lesson?.SettingsJson != null
                ? JsonSerializer.Deserialize<ExamSettingsJson>(lesson.SettingsJson)
                : null;

            return Ok(new
            {
                attemptId = attempt.Id,
                attempt.Score,
                attempt.Passed,
                attempt.Status,
                attempt.SubmittedAt,
                attempt.AttemptNumber,
                totalQuestions = attempt.QuizAttemptAnswers.Count,
                answeredCorrect = attempt.QuizAttemptAnswers.Count(a => a.IsCorrect == true),
                passingScore = examSettings?.PassingScore ?? 70,
                breakdown = attempt.QuizAttemptAnswers.Select(a => new
                {
                    a.QuestionId,
                    a.QuestionType,
                    a.UserAnswer,
                    a.IsCorrect,
                    a.PointsEarned,
                    a.PointsPossible
                })
            });
        }

        private bool ScoreQuestion(QuestionJson q, string? userAnswer)
        {
            if (userAnswer == null) return false;

            return q.Type switch
            {
                "multiple_choice" => int.TryParse(userAnswer, out var idx) && idx == q.CorrectIndex,
                "true_false" => bool.TryParse(userAnswer, out var b) &&
                                      q.CorrectAnswer.HasValue && b == q.CorrectAnswer.Value.GetBoolean(),
                "fill_in_blank" => q.CorrectAnswer.HasValue &&
                                      string.Equals(userAnswer, q.CorrectAnswer.Value.GetString(),
                                      StringComparison.OrdinalIgnoreCase),
                "multiple_select" => (JsonSerializer.Deserialize<List<int>>(userAnswer)?
                                          .OrderBy(x => x)
                                          .SequenceEqual(
                                              (q.CorrectIndices ?? []).OrderBy(x => x)
                                          ) ?? false),
                _ => false
            };
        }
    }
}
