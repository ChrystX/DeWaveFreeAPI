using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace DeWaveFreeAPI.Models;

[Table("student_courses")]
[PrimaryKey("StudentId", "CourseId")]
public partial class StudentCourse
{
    [Key]
    [Column("student_id")]
    public int StudentId { get; set; }

    [Key]
    [Column("course_id")]
    public int CourseId { get; set; }

    [Column("enrolled_at")]
    public DateTime EnrolledAt { get; set; }

    [Column("is_active")]
    public bool IsActive { get; set; }

    [Column("completed_at", TypeName = "datetime")]
    public DateTime? CompletedAt { get; set; }

    [Column("progress_percent", TypeName = "decimal(5, 2)")]
    public decimal ProgressPercent { get; set; }

    [Column("last_accessed_at", TypeName = "datetime")]
    public DateTime? LastAccessedAt { get; set; }

    [Column("certificate_issued_at", TypeName = "datetime")]
    public DateTime? CertificateIssuedAt { get; set; }

    [ForeignKey("StudentId")]
    [InverseProperty("StudentCourses")]
    public virtual Student Student { get; set; } = null!;

    [ForeignKey("CourseId")]
    [InverseProperty("StudentCourses")]
    public virtual Course Course { get; set; } = null!;
}
