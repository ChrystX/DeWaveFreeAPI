using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using DeWaveFreeAPI.Models;
using Microsoft.EntityFrameworkCore;

namespace DeWaveFreeAPI.Models;

[Table("course_events")]
[Index("CreatedByUserId", "StartTime", Name = "idx_course_events_created_by")]
[Index("Visibility", "StartTime", Name = "idx_course_events_visibility_time")]
public partial class CourseEvent
{
    [Key]
    [Column("id")]
    public int Id { get; set; }

    [Column("title")]
    [StringLength(255)]
    public string Title { get; set; } = null!;

    [Column("description")]
    public string? Description { get; set; }

    [Column("start_time")]
    public DateTime StartTime { get; set; }

    [Column("end_time")]
    public DateTime EndTime { get; set; }

    [Column("event_type")]
    [StringLength(20)]
    public string EventType { get; set; } = null!;

    [Column("visibility")]
    [StringLength(20)]
    public string Visibility { get; set; } = null!;

    [Column("track_attendance")]
    public bool TrackAttendance { get; set; }

    [Column("requires_registration")]
    public bool RequiresRegistration { get; set; }

    [Column("meeting_url")]
    [StringLength(255)]
    public string? MeetingUrl { get; set; }

    [Column("location")]
    [StringLength(255)]
    public string? Location { get; set; }

    [Column("thumbnail_url")]
    [StringLength(255)]
    public string? ThumbnailUrl { get; set; }

    [Column("color")]
    [StringLength(20)]
    public string? Color { get; set; }

    [Column("preview_video_url")]
    [StringLength(255)]
    public string? PreviewVideoUrl { get; set; }

    [Column("capacity")]
    public int? Capacity { get; set; }

    [Column("is_active")]
    public bool IsActive { get; set; }

    [Column("created_at")]
    public DateTime CreatedAt { get; set; }

    [Column("created_by_user_id")]
    public int CreatedByUserId { get; set; }

    [InverseProperty("Event")]
    public virtual ICollection<CourseEventCourse> CourseEventCourses { get; set; } = new List<CourseEventCourse>();

    [ForeignKey("CreatedByUserId")]
    [InverseProperty("CourseEvents")]
    public virtual User CreatedByUser { get; set; } = null!;

    [InverseProperty("Event")]
    public virtual ICollection<EventAttendance> EventAttendances { get; set; } = new List<EventAttendance>();

    [InverseProperty("Event")]
    public virtual ICollection<EventEnrollment> EventEnrollments { get; set; } = new List<EventEnrollment>();
}
