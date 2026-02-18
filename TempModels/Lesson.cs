using DeWaveFreeAPI.Models;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace DeWaveFreeAPI.TempModels;

[Table("lessons")]
[Index("SectionId", "SortOrder", Name = "idx_lessons_section")]
public partial class Lesson
{
    [Key]
    [Column("id")]
    public int Id { get; set; }

    [Column("section_id")]
    public int SectionId { get; set; }

    [Column("title")]
    [StringLength(255)]
    public string Title { get; set; } = null!;

    [Column("description")]
    public string? Description { get; set; }

    [Column("sort_order")]
    public int SortOrder { get; set; }

    [Column("created_at", TypeName = "datetime")]
    public DateTime? CreatedAt { get; set; }

    [Column("lesson_type")]
    [StringLength(10)]
    [Unicode(false)]
    public string LessonType { get; set; } = null!;

    [Column("settings_json")]
    public string? SettingsJson { get; set; }

    [InverseProperty("Lesson")]
    public virtual ICollection<QuizAttempt> QuizAttempts { get; set; } = new List<QuizAttempt>();

    [InverseProperty("Lesson")]
    public virtual ICollection<StudentLessonProgress> StudentLessonProgresses { get; set; } = new List<StudentLessonProgress>();

    [ForeignKey("SectionId")]
    [InverseProperty("Lessons")]
    public virtual CourseLearningSection Section { get; set; } = null!;
}
