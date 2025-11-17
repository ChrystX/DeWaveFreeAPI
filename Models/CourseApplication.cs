using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace DeWaveFreeAPI.Models;

public partial class CourseApplication
{
    [Key]
    public int Id { get; set; }

    [StringLength(255)]
    public string Name { get; set; } = null!;

    [StringLength(50)]
    public string Phone { get; set; } = null!;

    [StringLength(255)]
    public string Email { get; set; } = null!;

    [StringLength(500)]
    public string Address { get; set; } = null!;

    [StringLength(100)]
    public string Course { get; set; } = null!;

    [Column("CVFileName")]
    [StringLength(255)]
    public string? CvfileName { get; set; }

    [Column("CVFilePath")]
    [StringLength(500)]
    public string? CvfilePath { get; set; }

    public DateTime? SubmittedAt { get; set; }

    [StringLength(50)]
    public string? Status { get; set; }
}
