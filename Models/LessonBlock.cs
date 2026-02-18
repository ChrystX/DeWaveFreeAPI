using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace DeWaveFreeAPI.Models;

[Table("lesson_blocks")]
[Index("LessonId", "OrderIndex", Name = "idx_blocks_lesson")]
public partial class LessonBlock
{
    [Key]
    [Column("id")]
    public int Id { get; set; }

    [Column("lesson_id")]
    public int LessonId { get; set; }

    [Column("forked_from_content_object_id")]
    public int? ForkedFromContentObjectId { get; set; }

    [Column("block_type_id")]
    public int BlockTypeId { get; set; }

    [Column("order_index")]
    public int OrderIndex { get; set; }

    [Column("data_json")]
    public string? DataJson { get; set; }

    [Column("created_at", TypeName = "datetime")]
    public DateTime? CreatedAt { get; set; }

    [Column("content_object_id")]
    public int? ContentObjectId { get; set; }

    [ForeignKey("BlockTypeId")]
    [InverseProperty("LessonBlocks")]
    public virtual BlockType BlockType { get; set; } = null!;

    [ForeignKey("LessonId")]
    [InverseProperty("LessonBlocks")]
    public virtual Lesson Lesson { get; set; } = null!;

    [ForeignKey("ContentObjectId")]
    [InverseProperty("LessonBlocks")]
    public virtual ContentObject? ContentObject { get; set; }

    [ForeignKey("ForkedFromContentObjectId")]
    public virtual ContentObject? ForkedFromContentObject { get; set; }
}
