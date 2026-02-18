using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace DeWaveFreeAPI.Models;

[Table("user_sequences")]
[Index("Role", "Month", "Year", Name = "UQ_UserSequences", IsUnique = true)]
public partial class UserSequence
{
    [Key]
    public int Id { get; set; }

    [StringLength(20)]
    public string Role { get; set; } = null!;

    [StringLength(2)]
    [Unicode(false)]
    public string RolePrefix { get; set; } = null!;

    public byte Month { get; set; }

    public short Year { get; set; }

    public int? LastSequence { get; set; }

    [Column(TypeName = "datetime")]
    public DateTime? CreatedAt { get; set; }
}
