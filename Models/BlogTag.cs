using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace DeWaveFreeAPI.Models;

[PrimaryKey("BlogId", "Tag")]
[Table("blog_tags")]
public partial class BlogTag
{
    [Key]
    [Column("blog_id")]
    public int BlogId { get; set; }

    [Key]
    [Column("tag")]
    [StringLength(50)]
    public string Tag { get; set; } = null!;

    [ForeignKey("BlogId")]
    [InverseProperty("BlogTags")]
    public virtual Blog Blog { get; set; } = null!;
}
