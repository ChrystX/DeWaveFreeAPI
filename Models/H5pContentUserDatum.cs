using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace DeWaveFreeAPI.Models;

[Table("h5p_content_user_data")]
[Index("UserId", "ContentId", "SubContentId", "DataId", Name = "UQ_h5p_user_content", IsUnique = true)]
public partial class H5pContentUserDatum
{
    [Key]
    [Column("id")]
    public int Id { get; set; }

    [Column("user_id")]
    public int UserId { get; set; }

    [Column("content_id")]
    public int ContentId { get; set; }

    [Column("sub_content_id")]
    [StringLength(50)]
    [Unicode(false)]
    public string SubContentId { get; set; } = null!;

    [Column("data_id")]
    [StringLength(255)]
    [Unicode(false)]
    public string DataId { get; set; } = null!;

    [Column("data")]
    public string? Data { get; set; }

    [Column("preload")]
    public bool Preload { get; set; }

    [Column("invalidate")]
    public bool Invalidate { get; set; }

    [Column("updated_at", TypeName = "datetime")]
    public DateTime UpdatedAt { get; set; }

    [ForeignKey("ContentId")]
    [InverseProperty("H5pContentUserData")]
    public virtual ContentObject Content { get; set; } = null!;
}
