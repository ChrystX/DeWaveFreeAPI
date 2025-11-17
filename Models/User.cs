using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace DeWaveFreeAPI.Models
{
    [Table("Users")] // Change from "users" to "Users" to match your table
    public partial class User
    {
        [Key]
        [Column("Id")] // Change from "id" to "Id"
        public int Id { get; set; }

        [Required]
        [Column("Username")] // Change from "username" to "Username"
        [StringLength(100)]
        public string Username { get; set; } = null!;

        [Required]
        [Column("PasswordHash")] // Change from "password_hash" to "PasswordHash"
        [StringLength(200)]
        public string PasswordHash { get; set; } = null!;

        [Required]
        [Column("Role")] // Change from "role" to "Role"
        [StringLength(50)]
        public string Role { get; set; } = "User";
    }
}