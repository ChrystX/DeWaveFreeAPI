using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace DeWaveFreeAPI.Models;

[Table("quiz_attempts")]
[Index("LessonId", Name = "IX_qa_lesson")]
[Index("UserId", Name = "IX_qa_user")]
[Index("UserId", "LessonId", "AttemptNumber", Name = "UQ_qa_attempt", IsUnique = true)]
public partial class QuizAttempt
{
    [Key]
    [Column("id")]
    public int Id { get; set; }

    [Column("user_id")]
    public int UserId { get; set; }

    [Column("attempt_number")]
    public int AttemptNumber { get; set; }

    [Column("status")]
    [StringLength(20)]
    [Unicode(false)]
    public string Status { get; set; } = null!;

    [Column("answers_json")]
    public string? AnswersJson { get; set; }

    [Column("score", TypeName = "decimal(5, 2)")]
    public decimal? Score { get; set; }

    [Column("passed")]
    public bool? Passed { get; set; }

    [Column("started_at")]
    public DateTime? StartedAt { get; set; }

    [Column("submitted_at")]
    public DateTime? SubmittedAt { get; set; }

    [Column("lesson_id")]
    public int? LessonId { get; set; }

    [ForeignKey("LessonId")]
    [InverseProperty("QuizAttempts")]
    public virtual Lesson? Lesson { get; set; }

    [InverseProperty("Attempt")]
    public virtual ICollection<QuizAttemptAnswer> QuizAttemptAnswers { get; set; } = new List<QuizAttemptAnswer>();
}
