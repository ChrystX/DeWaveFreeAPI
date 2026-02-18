using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace DeWaveFreeAPI.Models;

[Table("student_lesson_progress")]
[Index("StudentId", "LessonId", Name = "UQ_student_lesson", IsUnique = true)]
[Index("LessonId", Name = "idx_slp_lesson_id")]
[Index("StudentId", Name = "idx_slp_student_id")]
public partial class StudentLessonProgress
{
    [Key]
    [Column("id")]
    public int Id { get; set; }

    [Column("student_id")]
    public int StudentId { get; set; }

    [Column("lesson_id")]
    public int LessonId { get; set; }

    [Column("completed_at", TypeName = "datetime")]
    public DateTime? CompletedAt { get; set; }

    [Column("status")]
    [StringLength(20)]
    [Unicode(false)]
    public string Status { get; set; } = null!;

    [Column("score", TypeName = "decimal(5, 2)")]
    public decimal? Score { get; set; }

    [Column("max_score", TypeName = "decimal(5, 2)")]
    public decimal? MaxScore { get; set; }

    [Column("started_at", TypeName = "datetime")]
    public DateTime? StartedAt { get; set; }

    [Column("last_accessed_at", TypeName = "datetime")]
    public DateTime? LastAccessedAt { get; set; }

    [ForeignKey("StudentId")]
    [InverseProperty("StudentLessonProgresses")]
    public virtual Student Student { get; set; } = null!;

    [ForeignKey("LessonId")]
    [InverseProperty("StudentLessonProgresses")]
    public virtual Lesson Lesson { get; set; } = null!;
}
