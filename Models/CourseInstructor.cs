using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace DeWaveFreeAPI.Models;

[Table("course_instructors")]
[PrimaryKey(nameof(CourseId), nameof(InstructorId))]
public partial class CourseInstructor
{
    [Column("course_id")]
    public int CourseId { get; set; }

    [Column("instructor_id")]
    public int InstructorId { get; set; }

    [Column("role")]
    [StringLength(100)]
    [Unicode(false)]
    public string? Role { get; set; }

    [Column("sort_order")]
    public int? SortOrder { get; set; }

    // Navigation properties - IMPORTANT: Make these nullable to avoid validation errors
    [ForeignKey("CourseId")]
    [InverseProperty("CourseInstructors")]
    public virtual Course? Course { get; set; } // Changed to nullable

    [ForeignKey("InstructorId")]
    [InverseProperty("CourseInstructors")]
    public virtual Instructor? Instructor { get; set; } // Changed to nullable
}