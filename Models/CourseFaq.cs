using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace DeWaveFreeAPI.Models;

[Table("course_faq")]
public partial class CourseFaq
{
    [Key]
    public int Id { get; set; }

    public int CourseId { get; set; }

    public string Question { get; set; } = null!;

    public string? Answer { get; set; }

    public int? SortOrder { get; set; }

    [ForeignKey("CourseId")]
    [InverseProperty("CourseFaqs")]
    public virtual Course Course { get; set; } = null!;
}
