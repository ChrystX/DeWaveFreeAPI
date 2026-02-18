using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace DeWaveFreeAPI.Models;

[Table("course_learning_sections")]
[Index("CourseId", "SortOrder", Name = "idx_learning_sections_course")]
public partial class CourseLearningSection
{
    [Key]
    [Column("id")]
    public int Id { get; set; }

    [Column("course_id")]
    public int CourseId { get; set; }

    [Column("title")]
    [StringLength(255)]
    public string Title { get; set; } = null!;

    [Column("sort_order")]
    public int SortOrder { get; set; }

    [Column("created_at", TypeName = "datetime")]
    public DateTime? CreatedAt { get; set; }

    [Column("is_active")]
    public bool IsActive { get; set; } = true;

    [ForeignKey("CourseId")]
    [InverseProperty("CourseLearningSections")]
    public virtual Course Course { get; set; } = null!;

    [InverseProperty("Section")]
    public virtual ICollection<Lesson> Lessons { get; set; } = new List<Lesson>();
}
