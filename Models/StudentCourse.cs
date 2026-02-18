using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace DeWaveFreeAPI.Models;

[PrimaryKey("StudentId", "CourseId")]
public partial class StudentCourse
{
    [Key]
    public int StudentId { get; set; }

    [Key]
    public int CourseId { get; set; }

    public DateTime EnrolledAt { get; set; }

    public bool IsActive { get; set; }

    [ForeignKey("StudentId")]
    [InverseProperty("StudentCourses")]
    public virtual Student Student { get; set; } = null!;
}
