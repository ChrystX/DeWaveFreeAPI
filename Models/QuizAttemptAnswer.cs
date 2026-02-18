using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace DeWaveFreeAPI.Models;

[Table("quiz_attempt_answers")]
[Index("AttemptId", Name = "IX_qaa_attempt")]
[Index("QuestionId", Name = "IX_qaa_question")]
public partial class QuizAttemptAnswer
{
    [Key]
    [Column("id")]
    public int Id { get; set; }

    [Column("attempt_id")]
    public int AttemptId { get; set; }

    [Column("question_id")]
    public int QuestionId { get; set; }

    [Column("question_type")]
    [StringLength(30)]
    [Unicode(false)]
    public string QuestionType { get; set; } = null!;

    [Column("user_answer")]
    public string? UserAnswer { get; set; }

    [Column("is_correct")]
    public bool? IsCorrect { get; set; }

    [Column("points_earned")]
    public int PointsEarned { get; set; }

    [Column("points_possible")]
    public int PointsPossible { get; set; }

    [ForeignKey("AttemptId")]
    [InverseProperty("QuizAttemptAnswers")]
    public virtual QuizAttempt Attempt { get; set; } = null!;
}
