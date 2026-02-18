using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace DeWaveFreeAPI.Models;

[Table("courses")]
public partial class Course
{
    [Column("title")]
    [StringLength(255)]
    [Unicode(false)]
    public string Title { get; set; } = null!;

    [Column("description", TypeName = "text")]
    public string? Description { get; set; }

    [Column("instructor")]
    [StringLength(255)]
    [Unicode(false)]
    public string? Instructor { get; set; }

    [Column("duration")]
    public int? Duration { get; set; }

    [Column("video_count")]
    public int? VideoCount { get; set; }

    [Column("rating", TypeName = "decimal(3, 2)")]
    public decimal? Rating { get; set; }

    [Column("image", TypeName = "text")]
    public string? Image { get; set; }

    [Column("IsActive")]
    public bool IsActive { get; set; } = true;

    [Column("created_at", TypeName = "datetime")]
    public DateTime? CreatedAt { get; set; }

    [Column("instructor_id")]
    public int InstructorId { get; set; }

    [Column("price", TypeName = "decimal(10,2)")]
    public decimal Price { get; set; }

    [Key]
    public int Id { get; set; }

    public int? CategoryId { get; set; }

    [ForeignKey("CategoryId")]
    [InverseProperty("Courses")]
    public virtual Category? Category { get; set; }

    [InverseProperty("Course")]
    public virtual ICollection<CourseDetail> CourseDetails { get; set; } = new List<CourseDetail>();

    [InverseProperty("Course")]
    public virtual ICollection<CourseFaq> CourseFaqs { get; set; } = new List<CourseFaq>();

    [InverseProperty("Course")]
    public virtual ICollection<CourseSection> CourseSections { get; set; } = new List<CourseSection>();

    [InverseProperty("Course")]
    public virtual ICollection<CourseInstructor> CourseInstructors { get; set; } = new List<CourseInstructor>();

    [InverseProperty("Course")]
    public virtual ICollection<Payment> Payments { get; set; } = new List<Payment>();

    [InverseProperty("Course")]
    public virtual ICollection<StudentCourse> StudentCourses { get; set; } = new List<StudentCourse>();

    [InverseProperty("Course")]
    public virtual ICollection<CourseLearningSection> CourseLearningSections { get; set; } = new List<CourseLearningSection>();

    [InverseProperty("Course")]
    public virtual ICollection<CourseEventCourse> CourseEventCourses { get; set; } = new List<CourseEventCourse>();
}
