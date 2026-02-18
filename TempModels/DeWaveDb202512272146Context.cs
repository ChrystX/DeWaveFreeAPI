using System;
using System.Collections.Generic;
using Microsoft.EntityFrameworkCore;

namespace DeWaveFreeAPI.TempModels;

public partial class DeWaveDb202512272146Context : DbContext
{
    public DeWaveDb202512272146Context()
    {
    }

    public DeWaveDb202512272146Context(DbContextOptions<DeWaveDb202512272146Context> options)
        : base(options)
    {
    }

    public virtual DbSet<Lesson> Lessons { get; set; }

    public virtual DbSet<QuizAttempt> QuizAttempts { get; set; }

    protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
#warning To protect potentially sensitive information in your connection string, you should move it out of source code. You can avoid scaffolding the connection string by using the Name= syntax to read it from configuration - see https://go.microsoft.com/fwlink/?linkid=2131148. For more guidance on storing connection strings, see https://go.microsoft.com/fwlink/?LinkId=723263.
        => optionsBuilder.UseSqlServer("Server=LAPTOP-T5PMAHFK;Database=de_wave_db-2025-12-27-21-46;Trusted_Connection=True;MultipleActiveResultSets=True;TrustServerCertificate=True;");

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<Lesson>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PK__lessons__3213E83F94DDB14A");

            entity.Property(e => e.CreatedAt).HasDefaultValueSql("(getdate())");
            entity.Property(e => e.LessonType).HasDefaultValue("lesson");
        });

        modelBuilder.Entity<QuizAttempt>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PK__quiz_att__3213E83FA35578F7");

            entity.Property(e => e.AttemptNumber).HasDefaultValue(1);
            entity.Property(e => e.StartedAt).HasDefaultValueSql("(getdate())");
            entity.Property(e => e.Status).HasDefaultValue("in_progress");

            entity.HasOne(d => d.Lesson).WithMany(p => p.QuizAttempts).HasConstraintName("FK_qa_lesson");
        });

        OnModelCreatingPartial(modelBuilder);
    }

    partial void OnModelCreatingPartial(ModelBuilder modelBuilder);
}
