using Microsoft.EntityFrameworkCore;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace DeWaveFreeAPI.Models;

[Table("course_instructors")]
[PrimaryKey(nameof(CourseId), nameof(InstructorId))]
public partial class CourseInstructor
{
    [Key]
    [Column("course_id")]
    public int CourseId { get; set; }

    [Key]
    [Column("instructor_id")]
    public int InstructorId { get; set; }

    [Column("sort_order")]
    public int? SortOrder { get; set; }

    [ForeignKey("CourseId")]
    [InverseProperty("CourseInstructors")]
    public virtual Course Course { get; set; } = null!;

    [ForeignKey("InstructorId")]
    [InverseProperty("CourseInstructors")]
    public virtual Instructor Instructor { get; set; } = null!;

}