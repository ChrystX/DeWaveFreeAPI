using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;
namespace DeWaveFreeAPI.Models;

[Table("content_objects")]
[Index("BlockTypeId", Name = "idx_content_objects_block_type")]
[Index("Title", Name = "idx_content_objects_title")]
public partial class ContentObject
{
    [Key]
    [Column("id")]
    public int Id { get; set; }

    [Column("title")]
    [StringLength(255)]
    public string Title { get; set; } = null!;

    [Column("block_type_id")]
    public int BlockTypeId { get; set; }

    [Column("data_json")]
    public string? DataJson { get; set; }

    [Column("version")]
    public int Version { get; set; }

    [Column("created_at", TypeName = "datetime")]
    public DateTime? CreatedAt { get; set; }

    [Column("h5p_library")]
    [StringLength(127)]
    [Unicode(false)]
    public string? H5pLibrary { get; set; }

    [Column("parent_id")]
    public int? ParentId { get; set; }

    [ForeignKey("ParentId")]
    public virtual ContentObject? Parent { get; set; }

    [Column("is_draft")]
    public bool IsDraft { get; set; }

    [Column("h5p_embed_type")]
    [StringLength(20)]
    [Unicode(false)]
    public string? H5pEmbedType { get; set; }

    [ForeignKey("BlockTypeId")]
    [InverseProperty("ContentObjects")]
    public virtual BlockType BlockType { get; set; } = null!;

    [InverseProperty("ContentObject")]
    public virtual ICollection<LessonBlock> LessonBlocks { get; set; } = new List<LessonBlock>();

    [InverseProperty("Content")]
    public virtual ICollection<H5pContentUserDatum> H5pContentUserData { get; set; } = new List<H5pContentUserDatum>();

    [InverseProperty("Parent")]
    public virtual ICollection<ContentObject> Versions { get; set; } = new List<ContentObject>();
}
