using System;
using System.Collections.Generic;
using Microsoft.EntityFrameworkCore;
using ZabgcExamsDesktop.MVVM.Model.DataBase.Models;

namespace ZabgcExamsDesktop.MVVM.Model.DataBase.Data;

public partial class ApplicationDbContext : DbContext
{
    public ApplicationDbContext()
    {
    }

    public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
        : base(options)
    {
    }

    public virtual DbSet<Audience> Audiences { get; set; }

    public virtual DbSet<Department> Departments { get; set; }

    public virtual DbSet<DepartmentOwner> DepartmentOwners { get; set; }

    public virtual DbSet<Discipline> Disciplines { get; set; }

    public virtual DbSet<Exam> Exams { get; set; }

    public virtual DbSet<Group> Groups { get; set; }

    public virtual DbSet<Manager> Managers { get; set; }

    public virtual DbSet<Qualification> Qualifications { get; set; }

    public virtual DbSet<Teacher> Teachers { get; set; }

    public virtual DbSet<TypeOfExam> TypeOfExams { get; set; }

    public virtual DbSet<TypeOfLesson> TypeOfLessons { get; set; }

    protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
#warning To protect potentially sensitive information in your connection string, you should move it out of source code. You can avoid scaffolding the connection string by using the Name= syntax to read it from configuration - see https://go.microsoft.com/fwlink/?linkid=2131148. For more guidance on storing connection strings, see https://go.microsoft.com/fwlink/?LinkId=723263.
        => optionsBuilder.UseSqlServer("Server=DESKTOP-AITO6UK\\SERVER;Database=ZabgcExams;Trusted_Connection=True;TrustServerCertificate=true;");

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<Audience>(entity =>
        {
            entity.HasKey(e => e.IdAudience).HasName("PK__Audience__80EB00415B92E8D7");

            entity.ToTable("Audience");

            entity.Property(e => e.IdAudience).HasColumnName("id_audience");
            entity.Property(e => e.NumberAudience).HasColumnName("Number_audience");
        });

        modelBuilder.Entity<Department>(entity =>
        {
            entity.HasKey(e => e.IdDepartment).HasName("PK__Departme__0FC1D23FC9D3B7B4");

            entity.ToTable("Department");

            entity.Property(e => e.IdDepartment).HasColumnName("id_department");
            entity.Property(e => e.NameOfDepartment)
                .HasMaxLength(50)
                .IsUnicode(false)
                .HasColumnName("Name_of_department");
        });

        modelBuilder.Entity<DepartmentOwner>(entity =>
        {
            entity.HasKey(e => e.IdOwner);

            entity.ToTable("DepartmentOwner");

            entity.Property(e => e.IdOwner).HasColumnName("id_owner");
            entity.Property(e => e.IdDepartment).HasColumnName("id_department");
            entity.Property(e => e.OwnerName)
                .HasMaxLength(50)
                .IsUnicode(false)
                .HasColumnName("owner_name");

            entity.HasOne(d => d.IdDepartmentNavigation).WithMany(p => p.DepartmentOwners)
                .HasForeignKey(d => d.IdDepartment)
                .HasConstraintName("FK_DepartmentOwner_Department");
        });

        modelBuilder.Entity<Discipline>(entity =>
        {
            entity.HasKey(e => e.IdDiscipline).HasName("PK__Discipli__3E5AFB6797F838E7");

            entity.ToTable("Discipline");

            entity.Property(e => e.IdDiscipline).HasColumnName("id_discipline");
            entity.Property(e => e.NameDiscipline)
                .IsUnicode(false)
                .HasColumnName("Name_discipline");
        });

        modelBuilder.Entity<Exam>(entity =>
        {
            entity.HasKey(e => e.IdExam).HasName("PK__Exam__5AF443195C19EC53");

            entity.ToTable("Exam");

            entity.Property(e => e.IdExam).HasColumnName("id_Exam");
            entity.Property(e => e.DateEvent)
                .HasColumnType("datetime")
                .HasColumnName("date_event");
            entity.Property(e => e.IdAudience).HasColumnName("id_audience");
            entity.Property(e => e.IdDiscipline).HasColumnName("id_discipline");
            entity.Property(e => e.IdGroup).HasColumnName("id_group");
            entity.Property(e => e.IdQualification).HasColumnName("id_qualification");
            entity.Property(e => e.IdTypeOfExam).HasColumnName("id_type_of_exam");
            entity.Property(e => e.IdTypeOfLesson).HasColumnName("id_type_of_lesson");

            entity.HasOne(d => d.IdAudienceNavigation).WithMany(p => p.Exams)
                .HasForeignKey(d => d.IdAudience)
                .HasConstraintName("FK_Exam_Audience");

            entity.HasOne(d => d.IdGroupNavigation).WithMany(p => p.Exams)
                .HasForeignKey(d => d.IdGroup)
                .HasConstraintName("FK_Exam_Group");

            entity.HasOne(d => d.IdQualificationNavigation).WithMany(p => p.Exams)
                .HasForeignKey(d => d.IdQualification)
                .HasConstraintName("FK_Exam_Qualification");

            entity.HasOne(d => d.IdTypeOfExamNavigation).WithMany(p => p.Exams)
                .HasForeignKey(d => d.IdTypeOfExam)
                .HasConstraintName("FK_Exam_Type_of_exam");

            entity.HasOne(d => d.IdDisciplineNavigation).WithMany(p => p.Exams)
                .HasForeignKey(d => d.IdTypeOfLesson)
                .HasConstraintName("FK_Exam_Discipline1");

            entity.HasOne(d => d.IdTypeOfLessonNavigation).WithMany(p => p.Exams)
                .HasForeignKey(d => d.IdTypeOfLesson)
                .HasConstraintName("FK_Exam_Type_of_lesson");
        });

        modelBuilder.Entity<Group>(entity =>
        {
            entity.HasKey(e => e.IdGroup).HasName("PK__Group__8BE8BA1BD972474B");

            entity.ToTable("Group");

            entity.Property(e => e.IdGroup).HasColumnName("id_group");
            entity.Property(e => e.IdDepartment).HasColumnName("id_department");
            entity.Property(e => e.NameOfGroup)
                .HasMaxLength(50)
                .IsUnicode(false)
                .HasColumnName("Name_of_group");

            entity.HasOne(d => d.IdDepartmentNavigation).WithMany(p => p.Groups)
                .HasForeignKey(d => d.IdDepartment)
                .OnDelete(DeleteBehavior.Cascade)
                .HasConstraintName("FK_Group_Department");
        });

        modelBuilder.Entity<Manager>(entity =>
        {
            entity.HasKey(e => e.IdManager);

            entity.Property(e => e.IdManager).HasColumnName("id_manager");
            entity.Property(e => e.FullName)
                .HasMaxLength(50)
                .IsUnicode(false)
                .HasColumnName("full_name");
            entity.Property(e => e.Post)
                .HasMaxLength(50)
                .IsUnicode(false)
                .HasColumnName("post");
        });

        modelBuilder.Entity<Qualification>(entity =>
        {
            entity.HasKey(e => e.IdQualification).HasName("PK__Qualific__DDDA3230C95692A1");

            entity.ToTable("Qualification");

            entity.Property(e => e.IdQualification).HasColumnName("id_qualification");
            entity.Property(e => e.NameQualification)
                .HasMaxLength(50)
                .IsUnicode(false)
                .HasColumnName("Name_qualification");
        });

        modelBuilder.Entity<Teacher>(entity =>
        {
            entity.HasKey(e => e.IdTeacher).HasName("PK__Teacher__3BAEF8F9173AC2E4");

            entity.ToTable("Teacher");

            entity.Property(e => e.IdTeacher).HasColumnName("id_teacher");
            entity.Property(e => e.FullName)
                .HasMaxLength(50)
                .IsUnicode(false)
                .HasColumnName("Full_name");

            entity.HasMany(d => d.IdExams).WithMany(p => p.IdTeachers)
                .UsingEntity<Dictionary<string, object>>(
                    "TeacherExam",
                    r => r.HasOne<Exam>().WithMany()
                        .HasForeignKey("IdExam")
                        .HasConstraintName("FK_Teacher_exam_Exam"),
                    l => l.HasOne<Teacher>().WithMany()
                        .HasForeignKey("IdTeacher")
                        .HasConstraintName("FK_Teacher_exam_Teacher"),
                    j =>
                    {
                        j.HasKey("IdTeacher", "IdExam").HasName("Teacher_exam_pk");
                        j.ToTable("Teacher_exam");
                        j.IndexerProperty<int>("IdTeacher").HasColumnName("id_teacher");
                        j.IndexerProperty<int>("IdExam").HasColumnName("id_exam");
                    });
        });

        modelBuilder.Entity<TypeOfExam>(entity =>
        {
            entity.HasKey(e => e.IdTypeOfExam).HasName("PK__Type_of___49A8A9EA31289D85");

            entity.ToTable("Type_of_exam");

            entity.Property(e => e.IdTypeOfExam).HasColumnName("id_type_of_exam");
            entity.Property(e => e.TypeOfExam1)
                .HasMaxLength(50)
                .IsUnicode(false)
                .HasColumnName("Type_of_exam");
        });

        modelBuilder.Entity<TypeOfLesson>(entity =>
        {
            entity.HasKey(e => e.IdTypeOfLesson).HasName("PK__Type_of___2BD18CF6C7B709CA");

            entity.ToTable("Type_of_lesson");

            entity.Property(e => e.IdTypeOfLesson).HasColumnName("id_type_of_lesson");
            entity.Property(e => e.TypeOfLesson1)
                .HasMaxLength(50)
                .IsUnicode(false)
                .HasColumnName("Type_of_lesson");
        });

        OnModelCreatingPartial(modelBuilder);
    }

    partial void OnModelCreatingPartial(ModelBuilder modelBuilder);
}
