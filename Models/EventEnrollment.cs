using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace DeWaveFreeAPI.Models;

[PrimaryKey("StudentId", "EventId")]
[Table("event_enrollments")]
[Index("EventId", "StudentId", Name = "idx_event_enrollments_event")]
public partial class EventEnrollment
{
    [Key]
    [Column("student_id")]
    public int StudentId { get; set; }

    [Key]
    [Column("event_id")]
    public int EventId { get; set; }

    [Column("registered_at")]
    public DateTime RegisteredAt { get; set; }

    [Column("status")]
    [StringLength(20)]
    public string Status { get; set; } = null!;

    [ForeignKey("EventId")]
    [InverseProperty("EventEnrollments")]
    public virtual CourseEvent Event { get; set; } = null!;

    [ForeignKey("StudentId")]
    [InverseProperty("EventEnrollments")]
    public virtual Student Student { get; set; } = null!;
}
