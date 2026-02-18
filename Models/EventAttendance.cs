using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace DeWaveFreeAPI.Models;

[PrimaryKey("StudentId", "EventId")]
[Table("event_attendance")]
[Index("EventId", "StudentId", Name = "idx_event_attendance_event")]
public partial class EventAttendance
{
    [Key]
    [Column("student_id")]
    public int StudentId { get; set; }

    [Key]
    [Column("event_id")]
    public int EventId { get; set; }

    [Column("attended")]
    public bool Attended { get; set; }

    [Column("joined_at")]
    public DateTime? JoinedAt { get; set; }

    [Column("status")]
    [StringLength(20)]
    public string Status { get; set; } = null!;

    [ForeignKey("EventId")]
    [InverseProperty("EventAttendances")]
    public virtual CourseEvent Event { get; set; } = null!;

    [ForeignKey("StudentId")]
    [InverseProperty("EventAttendances")]
    public virtual Student Student { get; set; } = null!;
}
