using Microsoft.EntityFrameworkCore;
using DeWaveFreeAPI.Data;
using DeWaveFreeAPI.DTOs.Events;
using DeWaveFreeAPI.Models;

namespace DeWaveFreeAPI.Services
{
    public interface IEventAttendanceService
    {
        // Mark Attendance (Instructor)
        Task<bool> MarkAttendanceAsync(MarkAttendanceDto dto);
        Task<bool> BulkMarkAttendanceAsync(BulkMarkAttendanceDto dto);

        // Student Check-in
        Task<bool> CheckInAsync(int eventId, int studentId);

        // Reports
        Task<AttendanceReportDto> GetAttendanceReportAsync(int eventId);
        Task<bool> HasStudentAttendedAsync(int eventId, int studentId);

        // Statistics
        Task<AttendanceStatsDto> GetAttendanceStatsAsync(int eventId);
    }
    public class EventAttendanceService : IEventAttendanceService
    {
        private readonly DeWaveAPIDbContext _context;
        private readonly ILogger<EventAttendanceService> _logger;

        public EventAttendanceService(DeWaveAPIDbContext context, ILogger<EventAttendanceService> logger)
        {
            _context = context;
            _logger = logger;
        }


        public async Task<bool> MarkAttendanceAsync(MarkAttendanceDto dto)
        {
            var courseEvent = await _context.CourseEvents.FindAsync(dto.EventId);
            if (courseEvent == null)
                throw new KeyNotFoundException($"Event {dto.EventId} not found.");

            if (!courseEvent.TrackAttendance)
                throw new InvalidOperationException("This event does not track attendance.");

            // Check if student is registered (if registration is required)
            if (courseEvent.RequiresRegistration)
            {
                var isRegistered = await _context.EventEnrollments
                    .AnyAsync(e => e.EventId == dto.EventId &&
                                  e.StudentId == dto.StudentId &&
                                  e.Status == "registered");

                if (!isRegistered)
                    throw new InvalidOperationException("Student is not registered for this event.");
            }

            // Get or create attendance record
            var attendance = await _context.EventAttendances
                .FirstOrDefaultAsync(a => a.EventId == dto.EventId && a.StudentId == dto.StudentId);

            if (attendance == null)
            {
                attendance = new EventAttendance
                {
                    EventId = dto.EventId,
                    StudentId = dto.StudentId,
                    Attended = dto.Attended,
                    JoinedAt = dto.Attended ? DateTime.UtcNow : null,
                    Status = dto.Attended ? "present" : "absent"
                };
                _context.EventAttendances.Add(attendance);
            }
            else
            {
                attendance.Attended = dto.Attended;
                attendance.JoinedAt = dto.Attended ? (attendance.JoinedAt ?? DateTime.UtcNow) : null;
                attendance.Status = dto.Attended ? "present" : "absent";
            }

            await _context.SaveChangesAsync();
            _logger.LogInformation($"Attendance marked for student {dto.StudentId} at event {dto.EventId}: {dto.Attended}");
            return true;
        }

        public async Task<bool> BulkMarkAttendanceAsync(BulkMarkAttendanceDto dto)
        {
            var courseEvent = await _context.CourseEvents.FindAsync(dto.EventId);
            if (courseEvent == null)
                throw new KeyNotFoundException($"Event {dto.EventId} not found.");

            if (!courseEvent.TrackAttendance)
                throw new InvalidOperationException("This event does not track attendance.");

            foreach (var studentAttendance in dto.Attendances)
            {
                var attendance = await _context.EventAttendances
                    .FirstOrDefaultAsync(a => a.EventId == dto.EventId && a.StudentId == studentAttendance.StudentId);

                if (attendance == null)
                {
                    attendance = new EventAttendance
                    {
                        EventId = dto.EventId,
                        StudentId = studentAttendance.StudentId,
                        Attended = studentAttendance.Attended,
                        JoinedAt = studentAttendance.JoinedAt ?? (studentAttendance.Attended ? DateTime.UtcNow : null),
                        Status = studentAttendance.Attended ? "present" : "absent"
                    };
                    _context.EventAttendances.Add(attendance);
                }
                else
                {
                    attendance.Attended = studentAttendance.Attended;
                    attendance.JoinedAt = studentAttendance.JoinedAt ?? attendance.JoinedAt;
                    attendance.Status = studentAttendance.Attended ? "present" : "absent";
                }
            }

            await _context.SaveChangesAsync();
            _logger.LogInformation($"Bulk attendance marked for event {dto.EventId}: {dto.Attendances.Count} students");
            return true;
        }

        public async Task<bool> CheckInAsync(int eventId, int studentId)
        {
            var courseEvent = await _context.CourseEvents.FindAsync(eventId);
            if (courseEvent == null)
                throw new KeyNotFoundException($"Event {eventId} not found.");

            if (!courseEvent.TrackAttendance)
                throw new InvalidOperationException("This event does not track attendance.");

            // Event must be happening now (within 30 minutes before start and before end)
            var now = DateTime.UtcNow;
            if (now < courseEvent.StartTime.AddMinutes(-30) || now > courseEvent.EndTime)
                throw new InvalidOperationException("Check-in is only available during the event.");

            // Check if registered (if required)
            if (courseEvent.RequiresRegistration)
            {
                var isRegistered = await _context.EventEnrollments
                    .AnyAsync(e => e.EventId == eventId &&
                                  e.StudentId == studentId &&
                                  e.Status == "registered");

                if (!isRegistered)
                    throw new InvalidOperationException("You must register for this event before checking in.");
            }

            // Get or create attendance
            var attendance = await _context.EventAttendances
                .FirstOrDefaultAsync(a => a.EventId == eventId && a.StudentId == studentId);

            if (attendance == null)
            {
                attendance = new EventAttendance
                {
                    EventId = eventId,
                    StudentId = studentId,
                    Attended = true,
                    JoinedAt = now,
                    Status = "present"
                };
                _context.EventAttendances.Add(attendance);
            }
            else
            {
                if (attendance.Attended)
                    throw new InvalidOperationException("You have already checked in.");

                attendance.Attended = true;
                attendance.JoinedAt = now;
                attendance.Status = "present";
            }

            await _context.SaveChangesAsync();
            _logger.LogInformation($"Student {studentId} checked in to event {eventId}");
            return true;
        }

        public async Task<AttendanceReportDto> GetAttendanceReportAsync(int eventId)
        {
            var courseEvent = await _context.CourseEvents
                .Include(e => e.EventEnrollments)
                    .ThenInclude(en => en.Student)
                        .ThenInclude(s => s.User)
                .Include(e => e.EventAttendances)
                    .ThenInclude(a => a.Student)
                        .ThenInclude(s => s.User)
                .FirstOrDefaultAsync(e => e.Id == eventId);

            if (courseEvent == null)
                throw new KeyNotFoundException($"Event {eventId} not found.");

            var students = new List<StudentAttendanceReportDto>();

            if (courseEvent.RequiresRegistration)
            {
                // Include all registered students
                foreach (var enrollment in courseEvent.EventEnrollments.Where(e => e.Status == "registered"))
                {
                    var attendance = courseEvent.EventAttendances.FirstOrDefault(a => a.StudentId == enrollment.StudentId);

                    students.Add(new StudentAttendanceReportDto
                    {
                        StudentId = enrollment.StudentId,
                        StudentName = enrollment.Student.FullName,
                        StudentDisplayId = enrollment.Student.User.DisplayId,
                        IsRegistered = true,
                        Attended = attendance?.Attended ?? false,
                        JoinedAt = attendance?.JoinedAt,
                        Status = attendance?.Status ?? "absent"
                    });
                }
            }
            else
            {
                // Include all students who checked in
                foreach (var attendance in courseEvent.EventAttendances)
                {
                    students.Add(new StudentAttendanceReportDto
                    {
                        StudentId = attendance.StudentId,
                        StudentName = attendance.Student.FullName,
                        StudentDisplayId = attendance.Student.User.DisplayId,
                        IsRegistered = false,
                        Attended = attendance.Attended,
                        JoinedAt = attendance.JoinedAt,
                        Status = attendance.Status
                    });
                }
            }

            var totalRegistered = students.Count(s => s.IsRegistered);
            var totalAttended = students.Count(s => s.Attended);
            var attendanceRate = totalRegistered > 0 ? (double)totalAttended / totalRegistered * 100 : 0;

            return new AttendanceReportDto
            {
                EventId = eventId,
                EventTitle = courseEvent.Title,
                EventDate = courseEvent.StartTime,
                TotalRegistered = totalRegistered,
                TotalAttended = totalAttended,
                TotalAbsent = totalRegistered - totalAttended,
                AttendanceRate = Math.Round(attendanceRate, 2),
                Students = students
            };
        }

        public async Task<bool> HasStudentAttendedAsync(int eventId, int studentId)
        {
            var attendance = await _context.EventAttendances
                .FirstOrDefaultAsync(a => a.EventId == eventId && a.StudentId == studentId);

            return attendance?.Attended ?? false;
        }

        public async Task<AttendanceStatsDto> GetAttendanceStatsAsync(int eventId)
        {
            var totalRegistered = await _context.EventEnrollments
                .CountAsync(e => e.EventId == eventId && e.Status == "registered");

            var totalAttended = await _context.EventAttendances
                .CountAsync(a => a.EventId == eventId && a.Attended);

            return new AttendanceStatsDto
            {
                TotalRegistered = totalRegistered,
                TotalAttended = totalAttended,
                AttendanceRate = totalRegistered > 0 ? (double)totalAttended / totalRegistered * 100 : 0
            };
        }
    }
}
