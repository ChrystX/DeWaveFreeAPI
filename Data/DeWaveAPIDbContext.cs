using System;
using System.Collections.Generic;
using DeWaveFreeAPI.Models;
using Microsoft.EntityFrameworkCore;

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

    public virtual DbSet<CourseFaq> CourseFaqs { get; set; }

    public virtual DbSet<CourseImage> CourseImages { get; set; }

    public virtual DbSet<CourseSection> CourseSections { get; set; }

    public virtual DbSet<Instructor> Instructors { get; set; }

    public virtual DbSet<Blog> Blogs { get; set; }

    public virtual DbSet<BlogDetail> BlogDetails { get; set; }

    public virtual DbSet<BlogTag> BlogTags { get; set; }

    public virtual DbSet<CourseApplication> CourseApplications { get; set; }

    public virtual DbSet<CourseInstructor> CourseInstructors { get; set; }

    public virtual DbSet<User> Users { get; set; }


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

            entity.Property(e => e.Id).ValueGeneratedNever();

            entity.HasOne(d => d.Course).WithMany(p => p.CourseDetails).HasConstraintName("FK_course_details_courses");
        });

        modelBuilder.Entity<CourseFaq>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PK__CourseFa__3214EC07576B2278");

            entity.HasOne(d => d.Course).WithMany(p => p.CourseFaqs).HasConstraintName("FK_CourseFaq_Course");
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

        modelBuilder.Entity<CourseApplication>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PK__CourseAp__3214EC0783C8197B");

            entity.Property(e => e.Status).HasDefaultValue("Pending");
            entity.Property(e => e.SubmittedAt).HasDefaultValueSql("(getutcdate())");
        });



        OnModelCreatingPartial(modelBuilder);
    }

    partial void OnModelCreatingPartial(ModelBuilder modelBuilder);
}
