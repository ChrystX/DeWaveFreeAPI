using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace DeWaveFreeAPI.Models;

[Table("course_images")]
public partial class CourseImage
{
    [Key]
    [Column("id")]
    [StringLength(24)]
    [Unicode(false)]
    public string Id { get; set; } = null!;

    [Column("detail_id")]
    public int? DetailId { get; set; }

    [Column("url", TypeName = "text")]
    public string Url { get; set; } = null!;

    [Column("caption", TypeName = "text")]
    public string? Caption { get; set; }

    [Column("order")]
    public int? Order { get; set; }

    [Column("is_main_image")]
    public bool? IsMainImage { get; set; }

    [Column("uploaded_at", TypeName = "datetime")]
    public DateTime? UploadedAt { get; set; }

    [ForeignKey("DetailId")]
    [InverseProperty("CourseImages")]
    public virtual CourseDetail? Detail { get; set; }
}
