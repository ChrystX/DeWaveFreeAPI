using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace DeWaveFreeAPI.Models;

[Table("instructors")]
public partial class Instructor
{
    [Column("name")]
    [StringLength(255)]
    [Unicode(false)]
    public string Name { get; set; } = null!;

    [Column("bio", TypeName = "text")]
    public string? Bio { get; set; }

    [Column("image_url")]
    [StringLength(512)]
    [Unicode(false)]
    public string? ImageUrl { get; set; }

    [Column("contact_email")]
    [StringLength(255)]
    [Unicode(false)]
    public string? ContactEmail { get; set; }

    [Column("phone_number")]
    [StringLength(14)]
    [Unicode(false)]
    public string? PhoneNumber { get; set; }

    [Column("certifications", TypeName = "text")]
    public string? Certifications { get; set; }

    [Key]
    public int Id { get; set; }

    [InverseProperty("Instructor")]
    public virtual ICollection<CourseInstructor> CourseInstructors { get; set; } = new List<CourseInstructor>();
}
