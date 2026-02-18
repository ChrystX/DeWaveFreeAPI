using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace DeWaveFreeAPI.Models;

[PrimaryKey("EventId", "CourseId")]
[Table("course_event_courses")]
[Index("CourseId", "EventId", Name = "idx_cec_course")]
[Index("EventId", Name = "idx_cec_event")]
public partial class CourseEventCourse
{
    [Key]
    [Column("event_id")]
    public int EventId { get; set; }

    [Key]
    [Column("course_id")]
    public int CourseId { get; set; }

    [Column("created_at")]
    public DateTime? CreatedAt { get; set; }

    [ForeignKey("CourseId")]
    [InverseProperty("CourseEventCourses")]
    public virtual Course Course { get; set; } = null!;

    [ForeignKey("EventId")]
    [InverseProperty("CourseEventCourses")]
    public virtual CourseEvent Event { get; set; } = null!;
}
