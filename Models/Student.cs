using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace DeWaveFreeAPI.Models;

[Index("UserId", Name = "UX_Students_UserId", IsUnique = true)]
public partial class Student
{
    [Key]
    public int Id { get; set; }

    public int UserId { get; set; }

    [Column("full_name")]
    [StringLength(255)]
    public string FullName { get; set; } = null!;

    [Column("phone_number")]
    [StringLength(14)]
    public string? PhoneNumber { get; set; }

    [Column("date_of_birth")]
    public DateTime? DateOfBirth { get; set; }

    [Column("address")]
    public string? Address { get; set; }

    [Column("emergency_contact")]
    [StringLength(255)]
    public string? EmergencyContact { get; set; }

    [Column("emergency_phone")]
    [StringLength(14)]
    public string? EmergencyPhone { get; set; }

    public DateTime CreatedAt { get; set; }

    public bool IsActive { get; set; }

    [InverseProperty("Student")]
    public virtual ICollection<Payment> Payments { get; set; } = new List<Payment>();

    [InverseProperty("Student")]
    public virtual ICollection<StudentCourse> StudentCourses { get; set; } = new List<StudentCourse>();

    [ForeignKey("UserId")]
    [InverseProperty("Student")]
    public virtual User User { get; set; } = null!;

    [InverseProperty("Student")] 
    public virtual ICollection<EventAttendance> EventAttendances { get; set; } = new List<EventAttendance>();

    [InverseProperty("Student")]
    public virtual ICollection<EventEnrollment> EventEnrollments { get; set; } = new List<EventEnrollment>();

    [InverseProperty("Student")]
    public virtual ICollection<StudentLessonProgress> StudentLessonProgresses { get; set; } = new List<StudentLessonProgress>();
}
