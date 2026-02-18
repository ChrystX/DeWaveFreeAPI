using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace DeWaveFreeAPI.Models;

[Table("course_details")]
public partial class CourseDetail
{
    [Key]
    [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
    [Column("id")]
    public int Id { get; set; }

    [Column("course_id")]
    public int? CourseId { get; set; }

    [StringLength(500)]
    public string? ShortDescription { get; set; }

    public string? FullDescriptionHtml { get; set; }

    public string? ToolsRequired { get; set; }

    [Column("hero_image")]
    public string? HeroImage { get; set; }

    [ForeignKey("CourseId")]
    [InverseProperty("CourseDetails")]
    public virtual Course? Course { get; set; }

    [InverseProperty("Detail")]
    public virtual ICollection<CourseImage> CourseImages { get; set; } = new List<CourseImage>();
}
