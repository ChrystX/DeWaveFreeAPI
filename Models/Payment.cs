using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace DeWaveFreeAPI.Models;

[Table("payments")]
public partial class Payment
{
    [Key]
    [Column("id")]
    public int Id { get; set; }

    [Column("student_id")]
    public int StudentId { get; set; }

    [Column("course_id")]
    public int CourseId { get; set; }

    [Column("amount", TypeName = "decimal(9, 2)")]
    public decimal Amount { get; set; }

    [Column("order_id")]
    [StringLength(100)]
    public string OrderId { get; set; } = null!;

    [Column("snap_token")]
    [StringLength(100)]
    public string? SnapToken { get; set; }

    [Column("status")]
    [StringLength(20)]
    public string Status { get; set; } = null!;

    [Column("created_at")]
    public DateTime CreatedAt { get; set; }

    [ForeignKey("StudentId")]
    [InverseProperty("Payments")]
    public virtual Student Student { get; set; } = null!;

    [ForeignKey("CourseId")]
    [InverseProperty("Payments")]
    public virtual Course Course { get; set; } = null!;
}