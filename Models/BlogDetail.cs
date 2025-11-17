using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace DeWaveFreeAPI.Models;

[Table("blog_details")]
public partial class BlogDetail
{
    [Key]
    [Column("blog_id")]
    public int BlogId { get; set; }

    [Column("content")]
    public string Content { get; set; } = null!;

    [Column("seo_title")]
    [StringLength(255)]
    public string? SeoTitle { get; set; }

    [Column("seo_description")]
    [StringLength(500)]
    public string? SeoDescription { get; set; }

    [Column("seo_keywords")]
    [StringLength(255)]
    public string? SeoKeywords { get; set; }

    [Column("tags")]
    [StringLength(255)]
    public string? Tags { get; set; }

    [Column("extra_json")]
    public string? ExtraJson { get; set; }

    [ForeignKey("BlogId")]
    [InverseProperty("BlogDetail")]
    public virtual Blog Blog { get; set; } = null!;
}
