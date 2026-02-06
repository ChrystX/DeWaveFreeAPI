using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace DeWaveFreeAPI.Models;

[Index("Email", Name = "IX_Users_Email")]
[Index("DisplayId", Name = "UQ_Users_DisplayId", IsUnique = true)]
[Index("Email", Name = "UQ_Users_Email", IsUnique = true)]
[Index("Username", Name = "UQ__Users__536C85E4FCAACD33", IsUnique = true)]
public partial class User
{
    [Key]
    public int Id { get; set; }

    [StringLength(100)]
    public string Username { get; set; } = null!;

    [StringLength(200)]
    public string PasswordHash { get; set; } = null!;

    [StringLength(255)]
    public string? Email { get; set; }

    public bool IsActive { get; set; }

    public bool IsEmailVerified { get; set; }

    [StringLength(255)]
    public string? EmailVerificationToken { get; set; }

    [StringLength(255)]
    public string? PasswordResetToken { get; set; }

    [Column(TypeName = "datetime")]
    public DateTime? PasswordResetTokenExpires { get; set; }

    [Column(TypeName = "datetime")]
    public DateTime CreatedAt { get; set; }

    [Column(TypeName = "datetime")]
    public DateTime? UpdatedAt { get; set; }

    [Column(TypeName = "datetime")]
    public DateTime? LastLoginAt { get; set; }

    [StringLength(10)]
    public string DisplayId { get; set; } = null!;

    public int RoleId { get; set; }

    [ForeignKey("RoleId")]
    [InverseProperty("Users")]
    public virtual Role Role { get; set; } = null!;
}
