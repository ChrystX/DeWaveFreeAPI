using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace DeWaveFreeAPI.Models;

[Table("block_types")]
[Index("Name", Name = "UQ__block_ty__72E12F1B3F33E3D0", IsUnique = true)]
public partial class BlockType
{
    [Key]
    [Column("id")]
    public int Id { get; set; }

    [Column("name")]
    [StringLength(50)]
    public string Name { get; set; } = null!;

    [Column("is_composite")]
    public bool IsComposite { get; set; } = false;

    [InverseProperty("BlockType")]
    public virtual ICollection<LessonBlock> LessonBlocks { get; set; } = new List<LessonBlock>();

    [InverseProperty("BlockType")]
    public virtual ICollection<ContentObject> ContentObjects { get; set; } = new List<ContentObject>();
}
