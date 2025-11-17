using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace DeWaveFreeAPI.Models;

[Table("course_section")]
public partial class CourseSection
{
    [Key]
    public int Id { get; set; }

    public int CourseId { get; set; }

    [StringLength(255)]
    public string Title { get; set; } = null!;

    public string? ContentHtml { get; set; }

    [StringLength(500)]
    public string? VideoUrl { get; set; }

    [StringLength(500)]
    public string? ThumbnailUrl { get; set; }

    public int? DurationMinutes { get; set; }

    public int SortOrder { get; set; }

    [ForeignKey("CourseId")]
    [InverseProperty("CourseSections")]
    public virtual Course Course { get; set; } = null!;
}
