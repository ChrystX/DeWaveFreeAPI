using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace DeWaveFreeAPI.Models;

[Table("course_faqs")]
public partial class CourseFaq
{
    [Key]
    [Column("id")]
    public int Id { get; set; }

    [Column("course_id")]
    public int CourseId { get; set; }

    [Column("question", TypeName = "text")]
    public string Question { get; set; } = null!;

    [Column("answer", TypeName = "text")]
    public string? Answer { get; set; }

    [Column("sort_order")]
    public int? SortOrder { get; set; }

    [ForeignKey("CourseId")]
    [InverseProperty("CourseFaqs")]
    public virtual Course Course { get; set; } = null!;
}