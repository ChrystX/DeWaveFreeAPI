using Microsoft.EntityFrameworkCore;
using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace DeWaveFreeAPI.Models;

[Index("Token", Name = "IX_RefreshTokens_Token", IsUnique = true)]
[Index("UserId", Name = "IX_RefreshTokens_UserId")]
[Table("refresh_tokens")]
public partial class RefreshToken
{
    [Key]
    public int Id { get; set; }

    [StringLength(200)]
    public string Token { get; set; } = null!;

    public int UserId { get; set; }

    [Column(TypeName = "datetime")]
    public DateTime ExpiresAt { get; set; }

    public bool IsRevoked { get; set; }

    [Column(TypeName = "datetime")]
    public DateTime CreatedAt { get; set; }

    [ForeignKey("UserId")]
    [InverseProperty("RefreshTokens")]
    public virtual User User { get; set; } = null!;
}