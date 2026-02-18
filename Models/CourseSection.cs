using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace DeWaveFreeAPI.Models;

[Table("course_sections")]
public partial class CourseSection
{
    [Key]
    [Column("id")]
    public int Id { get; set; }

    [Column("course_id")]
    public int CourseId { get; set; }

    [Column("title")]
    [StringLength(255)]
    public string Title { get; set; } = null!;

    [Column("content_html", TypeName = "text")]
    public string? ContentHtml { get; set; }

    [Column("video_url")]
    [StringLength(500)]
    public string? VideoUrl { get; set; }

    [Column("thumbnail_url")]
    [StringLength(500)]
    public string? ThumbnailUrl { get; set; }

    [Column("duration_minutes")]
    public int? DurationMinutes { get; set; }

    [Column("sort_order")]
    public int SortOrder { get; set; }

    [ForeignKey("CourseId")]
    [InverseProperty("CourseSections")]
    public virtual Course Course { get; set; } = null!;
}