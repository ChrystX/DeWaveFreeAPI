using DeWaveFreeAPI.Models;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;

namespace DeWaveFreeAPI.Data;

public partial class DeWaveAPIDbContext : DbContext
{
    public DeWaveAPIDbContext()
    {
    }

    public DeWaveAPIDbContext(DbContextOptions<DeWaveAPIDbContext> options)
        : base(options)
    {
    }

    public virtual DbSet<Category> Categories { get; set; }

    public virtual DbSet<Course> Courses { get; set; }

    public virtual DbSet<CourseDetail> CourseDetails { get; set; }

    public virtual DbSet<H5pContentUserDatum> H5pContentUserData { get; set; }

    public virtual DbSet<QuizAttempt> QuizAttempts { get; set; }

    public virtual DbSet<QuizAttemptAnswer> QuizAttemptAnswers { get; set; }

    public virtual DbSet<CourseFaq> CourseFaqs { get; set; }

    public virtual DbSet<CourseImage> CourseImages { get; set; }

    public virtual DbSet<CourseSection> CourseSections { get; set; }

    public virtual DbSet<Instructor> Instructors { get; set; }

    public virtual DbSet<Blog> Blogs { get; set; }

    public virtual DbSet<BlogDetail> BlogDetails { get; set; }

    public virtual DbSet<BlogTag> BlogTags { get; set; }

    public virtual DbSet<CourseApplication> CourseApplications { get; set; }

    public virtual DbSet<CourseInstructor> CourseInstructors { get; set; }

    public virtual DbSet<Role> Roles { get; set; }

    public virtual DbSet<User> Users { get; set; }

    public virtual DbSet<InstructorType> InstructorTypes { get; set; }

    public virtual DbSet<Payment> Payments { get; set; }

    public virtual DbSet<UserSequence> UserSequences { get; set; }

    public virtual DbSet<Student> Students { get; set; }

    public virtual DbSet<StudentCourse> StudentCourses { get; set; }

    public virtual DbSet<RefreshToken> RefreshTokens { get; set; }

    public virtual DbSet<CourseLearningSection> CourseLearningSections { get; set; }

    public virtual DbSet<ContentObject> ContentObjects { get; set; }

    public virtual DbSet<Lesson> Lessons { get; set; }

    public virtual DbSet<LessonBlock> LessonBlocks { get; set; }

    public virtual DbSet<BlockType> BlockTypes { get; set; }

    public virtual DbSet<StudentLessonProgress> StudentLessonProgresses { get; set; }

    public virtual DbSet<CourseEventCourse> CourseEventCourses { get; set; }

    public virtual DbSet<CourseEvent> CourseEvents { get; set; }

    public virtual DbSet<EventAttendance> EventAttendances { get; set; }

    public virtual DbSet<EventEnrollment> EventEnrollments { get; set; }


    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<Category>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PK__Category__3214EC0713EA737F");
        });

        modelBuilder.Entity<Course>(entity =>
        {
            entity.Property(e => e.CreatedAt).HasDefaultValueSql("(getdate())");

            entity.HasOne(d => d.Category).WithMany(p => p.Courses).HasConstraintName("FK_Course_Category");
        });

        modelBuilder.Entity<CourseDetail>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PK__course_d__3213E83F07F62793");

            entity.Property(e => e.Id).ValueGeneratedOnAdd();

            entity.HasOne(d => d.Course).WithMany(p => p.CourseDetails).HasConstraintName("FK_course_details_courses");
        });

        modelBuilder.Entity<Payment>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PK__Payments__3214EC075C33B68C");

            entity.Property(e => e.CreatedAt).HasDefaultValueSql("(getutcdate())");
            entity.Property(e => e.Status).HasDefaultValue("Pending");

            entity.HasOne(d => d.Student).WithMany(p => p.Payments)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_Payments_Student");
        });

        modelBuilder.Entity<QuizAttempt>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PK__quiz_att__3213E83FA35578F7");

            entity.Property(e => e.AttemptNumber).HasDefaultValue(1);
            entity.Property(e => e.StartedAt).HasDefaultValueSql("(getdate())");
            entity.Property(e => e.Status).HasDefaultValue("in_progress");

            entity.HasOne(d => d.Lesson).WithMany(p => p.QuizAttempts).HasConstraintName("FK_qa_lesson");
        });

        modelBuilder.Entity<CourseFaq>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PK__CourseFa__3214EC07576B2278");

            entity.HasOne(d => d.Course).WithMany(p => p.CourseFaqs).HasConstraintName("FK_CourseFaq_Course");
        });

        modelBuilder.Entity<Student>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PK__Students__3214EC07B92067FA");

            entity.Property(e => e.CreatedAt).HasDefaultValueSql("(getutcdate())");
            entity.Property(e => e.IsActive).HasDefaultValue(true);

            entity.HasOne(d => d.User).WithOne(p => p.Student).HasConstraintName("FK_Students_Users");
        });

        modelBuilder.Entity<StudentCourse>(entity =>
        {
            entity.Property(e => e.EnrolledAt).HasDefaultValueSql("(getutcdate())");
            entity.Property(e => e.IsActive).HasDefaultValue(true);

            //entity.HasOne(d => d.Student).WithMany(p => p.StudentCourses).HasConstraintName("FK_StudentCourses_Students");
        });

        modelBuilder.Entity<InstructorType>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PK__instruct__3213E83F07A0CDBA");
        });

        modelBuilder.Entity<CourseImage>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PK__course_i__3213E83F9F132558");

            entity.Property(e => e.IsMainImage).HasDefaultValue(false);
            entity.Property(e => e.Order).HasDefaultValue(0);
            entity.Property(e => e.UploadedAt).HasDefaultValueSql("(getdate())");

            entity.HasOne(d => d.Detail).WithMany(p => p.CourseImages)
                .OnDelete(DeleteBehavior.Cascade)
                .HasConstraintName("FK__course_im__detai__6754599E");
        });

        modelBuilder.Entity<CourseEvent>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PK__course_e__3213E83F82862F97");

            entity.HasIndex(e => e.StartTime, "idx_course_events_active").HasFilter("([is_active]=(1))");

            entity.Property(e => e.CreatedAt).HasDefaultValueSql("(getutcdate())");
            entity.Property(e => e.EventType).HasDefaultValue("online");
            entity.Property(e => e.IsActive).HasDefaultValue(true);
            entity.Property(e => e.TrackAttendance).HasDefaultValue(true);
            entity.Property(e => e.Visibility).HasDefaultValue("course_only");

            entity.HasOne(d => d.CreatedByUser).WithMany(p => p.CourseEvents)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("fk_course_events_user");
        });

        modelBuilder.Entity<CourseEventCourse>(entity =>
        {
            entity.HasKey(e => new { e.EventId, e.CourseId }).HasName("PK__course_e__DB81185DFE7574B4");

            entity.Property(e => e.CreatedAt).HasDefaultValueSql("(getutcdate())");

            entity.HasOne(d => d.Course).WithMany(p => p.CourseEventCourses)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("fk_cec_course");

            entity.HasOne(d => d.Event).WithMany(p => p.CourseEventCourses)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("fk_cec_event");
        });

        modelBuilder.Entity<EventAttendance>(entity =>
        {
            entity.HasKey(e => new { e.StudentId, e.EventId }).HasName("PK__event_at__480409E89F7B98DE");

            entity.Property(e => e.Status).HasDefaultValue("absent");

            entity.HasOne(d => d.Event).WithMany(p => p.EventAttendances)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("fk_event_attendance_event");

            entity.HasOne(d => d.Student).WithMany(p => p.EventAttendances)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("fk_event_attendance_student");
        });

        modelBuilder.Entity<H5pContentUserDatum>(entity =>
        {
            entity.Property(e => e.SubContentId).HasDefaultValue("0");

            entity.HasOne(d => d.Content).WithMany(p => p.H5pContentUserData)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_h5p_content_user_data_content");
        });

        modelBuilder.Entity<EventEnrollment>(entity =>
        {
            entity.HasKey(e => new { e.StudentId, e.EventId }).HasName("PK__event_en__480409E874514955");

            entity.Property(e => e.RegisteredAt).HasDefaultValueSql("(getutcdate())");
            entity.Property(e => e.Status).HasDefaultValue("registered");

            entity.HasOne(d => d.Event).WithMany(p => p.EventEnrollments)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("fk_event_enrollments_event");

            entity.HasOne(d => d.Student).WithMany(p => p.EventEnrollments)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("fk_event_enrollments_student");
        });

        modelBuilder.Entity<CourseSection>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PK__CourseSy__3214EC07BFACF63A");

            entity.HasOne(d => d.Course).WithMany(p => p.CourseSections).HasConstraintName("FK_CourseSyllabus_Course");
        });

        modelBuilder.Entity<CourseInstructor>(entity =>
        {
            entity.HasKey(e => new { e.CourseId, e.InstructorId })
                .HasName("PK_course_instructors");

            entity.HasOne(d => d.Course)
                .WithMany(p => p.CourseInstructors)
                .HasForeignKey(d => d.CourseId)
                .OnDelete(DeleteBehavior.Cascade)
                .HasConstraintName("FK_course_instructors_courses");

            entity.HasOne(d => d.Instructor)
                .WithMany(p => p.CourseInstructors)
                .HasForeignKey(d => d.InstructorId)
                .OnDelete(DeleteBehavior.Cascade)
                .HasConstraintName("FK_course_instructors_instructors");
        });


        modelBuilder.Entity<Instructor>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PK__instruct__3214EC070B3A630E");

            entity.Property(e => e.PhoneNumber).IsFixedLength();
        });

        modelBuilder.Entity<Blog>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PK__blog__3213E83FF4ACE46E");

            entity.Property(e => e.CreatedAt).HasDefaultValueSql("(sysdatetime())");
            entity.Property(e => e.Status).HasDefaultValue("draft");
            entity.Property(e => e.UpdatedAt).HasDefaultValueSql("(sysdatetime())");
        });

        modelBuilder.Entity<BlogDetail>(entity =>
        {
            entity.HasKey(e => e.BlogId).HasName("PK__blog_det__2975AA2855C60D32");

            entity.Property(e => e.BlogId).ValueGeneratedNever();

            entity.HasOne(d => d.Blog).WithOne(p => p.BlogDetail).HasConstraintName("FK_blog_details_blog");
        });

        modelBuilder.Entity<BlogTag>(entity =>
        {
            entity.HasKey(e => new { e.BlogId, e.Tag }).HasName("PK__blog_tag__34B4ABE8F7FE4C7E");

            entity.HasOne(d => d.Blog).WithMany(p => p.BlogTags).HasConstraintName("FK_blog_tags_blog");
        });

        modelBuilder.Entity<ContentObject>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PK__content___3213E83F38580BB9");

            entity.Property(e => e.CreatedAt).HasDefaultValueSql("(getdate())");
            entity.Property(e => e.Version).HasDefaultValue(1);

            entity.HasOne(d => d.BlockType).WithMany(p => p.ContentObjects)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_content_objects_block_types");
        });

        modelBuilder.Entity<CourseApplication>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PK__CourseAp__3214EC0783C8197B");

            entity.Property(e => e.Status).HasDefaultValue("Pending");
            entity.Property(e => e.SubmittedAt).HasDefaultValueSql("(getutcdate())");
        });

        modelBuilder.Entity<Role>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PK__Roles__3214EC075882ED90");
        });

        modelBuilder.Entity<User>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PK__Users__3214EC072DC8FA20");

            entity.Property(e => e.CreatedAt).HasDefaultValueSql("(getdate())");
            entity.Property(e => e.IsActive).HasDefaultValue(true);

            entity.HasOne(d => d.Role).WithMany(p => p.Users)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_Users_Roles");
        });

        modelBuilder.Entity<RefreshToken>(entity =>
        {
            entity.ToTable("refresh_tokens");

            entity.HasKey(e => e.Id).HasName("PK__RefreshT__3214EC07F8A50CB2");

            entity.Property(e => e.CreatedAt).HasDefaultValueSql("(getutcdate())");

            entity.HasOne(d => d.User).WithMany(p => p.RefreshTokens).HasConstraintName("FK_RefreshTokens_Users");
        });

        modelBuilder.Entity<UserSequence>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PK__UserSequ__3214EC07E4B097EC");

            entity.Property(e => e.CreatedAt).HasDefaultValueSql("(getdate())");
            entity.Property(e => e.LastSequence).HasDefaultValue(0);
            entity.Property(e => e.RolePrefix).IsFixedLength();
        });

        modelBuilder.Entity<CourseLearningSection>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PK__course_l__3213E83F738C8CE9");

            entity.Property(e => e.CreatedAt).HasDefaultValueSql("(getdate())");

            entity.HasOne(d => d.Course).WithMany(p => p.CourseLearningSections).HasConstraintName("FK_learning_sections_course");
        });

        modelBuilder.Entity<Lesson>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PK__lessons__3213E83F94DDB14A");

            entity.Property(e => e.CreatedAt).HasDefaultValueSql("(getdate())");

            entity.HasOne(d => d.Section).WithMany(p => p.Lessons).HasConstraintName("FK_lessons_section");
        });

        modelBuilder.Entity<LessonBlock>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PK__lesson_b__3213E83FFCC61603");

            entity.Property(e => e.CreatedAt).HasDefaultValueSql("(getdate())");

            entity.HasOne(d => d.BlockType).WithMany(p => p.LessonBlocks)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_blocks_type");

            entity.HasOne(d => d.Lesson).WithMany(p => p.LessonBlocks).HasConstraintName("FK_blocks_lesson");
        });

        modelBuilder.Entity<BlockType>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PK__block_ty__3213E83FDC459AAD");
        });

        modelBuilder.Entity<StudentLessonProgress>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PK__student___3213E83F5ED50D46");

            entity.Property(e => e.CompletedAt).HasDefaultValueSql("(getdate())");

            entity.HasOne(d => d.Student).WithMany(p => p.StudentLessonProgresses).HasConstraintName("FK_slp_student");
        });


        OnModelCreatingPartial(modelBuilder);
    }

    partial void OnModelCreatingPartial(ModelBuilder modelBuilder);
}
