using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace DeWaveFreeAPI.Models;

[Table("blog")]
[Index("Slug", Name = "UQ__blog__32DD1E4C9A687D8B", IsUnique = true)]
public partial class Blog
{
    [Key]
    [Column("id")]
    public int Id { get; set; }

    [Column("title")]
    [StringLength(255)]
    public string Title { get; set; } = null!;

    [Column("slug")]
    [StringLength(255)]
    public string Slug { get; set; } = null!;

    [Column("author_id")]
    public int? AuthorId { get; set; }

    [Column("summary")]
    public string? Summary { get; set; }

    [Column("thumbnail_url")]
    [StringLength(255)]
    public string? ThumbnailUrl { get; set; }

    [Column("category_id")]
    public int? CategoryId { get; set; }

    [Column("status")]
    [StringLength(20)]
    public string Status { get; set; } = null!;

    [Column("created_at")]
    public DateTime CreatedAt { get; set; }

    [Column("updated_at")]
    public DateTime UpdatedAt { get; set; }

    [Column("published_at")]
    public DateTime? PublishedAt { get; set; }

    [Column("view_count")]
    public int ViewCount { get; set; } = 0;

    [InverseProperty("Blog")]
    public virtual BlogDetail? BlogDetail { get; set; }

    [InverseProperty("Blog")]
    public virtual ICollection<BlogTag> BlogTags { get; set; } = new List<BlogTag>();
}
