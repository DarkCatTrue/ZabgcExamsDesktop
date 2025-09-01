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

    public virtual DbSet<Discipline> Disciplines { get; set; }

    public virtual DbSet<Exam> Exams { get; set; }

    public virtual DbSet<GetAllAudience> GetAllAudiences { get; set; }

    public virtual DbSet<GetAllDepartment> GetAllDepartments { get; set; }

    public virtual DbSet<GetAllDiscipline> GetAllDisciplines { get; set; }

    public virtual DbSet<GetAllExam> GetAllExams { get; set; }

    public virtual DbSet<GetAllGroup> GetAllGroups { get; set; }

    public virtual DbSet<GetAllTeacher> GetAllTeachers { get; set; }

    public virtual DbSet<GetTypeExam> GetTypeExams { get; set; }

    public virtual DbSet<GetTypeLesson> GetTypeLessons { get; set; }

    public virtual DbSet<Group> Groups { get; set; }

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
            entity.HasKey(e => e.IdAudience).HasName("PK__Audience__80EB004116F980A6");

            entity.ToTable("Audience");

            entity.Property(e => e.IdAudience).HasColumnName("id_audience");
            entity.Property(e => e.NumberAudience).HasColumnName("Number_audience");

            entity.HasMany(d => d.IdExams).WithMany(p => p.IdAudiences)
                .UsingEntity<Dictionary<string, object>>(
                    "AudienceExam",
                    r => r.HasOne<Exam>().WithMany()
                        .HasForeignKey("IdExam")
                        .OnDelete(DeleteBehavior.ClientSetNull)
                        .HasConstraintName("FK__Audience___id_ex__6FE99F9F"),
                    l => l.HasOne<Audience>().WithMany()
                        .HasForeignKey("IdAudience")
                        .OnDelete(DeleteBehavior.ClientSetNull)
                        .HasConstraintName("FK__Audience___id_au__6EF57B66"),
                    j =>
                    {
                        j.HasKey("IdAudience", "IdExam").HasName("Audience_exam_pk");
                        j.ToTable("Audience_exam");
                        j.IndexerProperty<int>("IdAudience").HasColumnName("id_audience");
                        j.IndexerProperty<int>("IdExam").HasColumnName("id_exam");
                    });
        });

        modelBuilder.Entity<Department>(entity =>
        {
            entity.HasKey(e => e.IdDepartment).HasName("PK__Departme__0FC1D23F53A4E6F7");

            entity.ToTable("Department");

            entity.Property(e => e.IdDepartment).HasColumnName("id_department");
            entity.Property(e => e.NameOfDepartment)
                .HasMaxLength(50)
                .IsUnicode(false)
                .HasColumnName("Name_of_department");
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
            entity.HasKey(e => e.IdExam).HasName("PK__Exam__5AF44319496340CD");

            entity.ToTable("Exam");

            entity.Property(e => e.IdExam).HasColumnName("id_Exam");
            entity.Property(e => e.DateEvent)
                .HasColumnType("datetime")
                .HasColumnName("date_event");
            entity.Property(e => e.IdDiscipline).HasColumnName("id_discipline");
            entity.Property(e => e.IdGroup).HasColumnName("id_group");
            entity.Property(e => e.IdQualification).HasColumnName("id_qualification");
            entity.Property(e => e.IdTypeOfExam).HasColumnName("id_type_of_exam");
            entity.Property(e => e.IdTypeOfLesson).HasColumnName("id_type_of_lesson");

            entity.HasOne(d => d.IdDisciplineNavigation).WithMany(p => p.Exams)
                .HasForeignKey(d => d.IdDiscipline)
                .HasConstraintName("FK_Disciplines_Exams");

            entity.HasOne(d => d.IdGroupNavigation).WithMany(p => p.Exams)
                .HasForeignKey(d => d.IdGroup)
                .HasConstraintName("FK_Groups_Exams");

            entity.HasOne(d => d.IdQualificationNavigation).WithMany(p => p.Exams)
                .HasForeignKey(d => d.IdQualification)
                .HasConstraintName("FK_qualification_exam");

            entity.HasOne(d => d.IdTypeOfExamNavigation).WithMany(p => p.Exams)
                .HasForeignKey(d => d.IdTypeOfExam)
                .HasConstraintName("FK_Type_of_exam_Exams");

            entity.HasOne(d => d.IdTypeOfLessonNavigation).WithMany(p => p.Exams)
                .HasForeignKey(d => d.IdTypeOfLesson)
                .HasConstraintName("FK_Type_of_lesson_Exams");
        });

        modelBuilder.Entity<GetAllAudience>(entity =>
        {
            entity
                .HasNoKey()
                .ToView("GetAllAudiences");

            entity.Property(e => e.NumberAudience).HasColumnName("Number_audience");
        });

        modelBuilder.Entity<GetAllDepartment>(entity =>
        {
            entity
                .HasNoKey()
                .ToView("GetAllDepartments");

            entity.Property(e => e.NameOfDepartment)
                .HasMaxLength(50)
                .IsUnicode(false)
                .HasColumnName("Name_of_department");
        });

        modelBuilder.Entity<GetAllDiscipline>(entity =>
        {
            entity
                .HasNoKey()
                .ToView("GetAllDisciplines");

            entity.Property(e => e.NameDiscipline)
                .IsUnicode(false)
                .HasColumnName("Name_discipline");
        });

        modelBuilder.Entity<GetAllExam>(entity =>
        {
            entity
                .HasNoKey()
                .ToView("GetAllExams");

            entity.Property(e => e.Department)
                .HasMaxLength(50)
                .IsUnicode(false);
            entity.Property(e => e.ExamId).HasColumnName("Exam_ID");
            entity.Property(e => e.GroupName)
                .HasMaxLength(50)
                .IsUnicode(false)
                .HasColumnName("Group_Name");
            entity.Property(e => e.NameDiscipline)
                .IsUnicode(false)
                .HasColumnName("Name_discipline");
            entity.Property(e => e.NumberAudience).HasColumnName("Number_audience");
            entity.Property(e => e.TeacherName)
                .HasMaxLength(50)
                .IsUnicode(false)
                .HasColumnName("Teacher_Name");
            entity.Property(e => e.TypeOfExam)
                .HasMaxLength(50)
                .IsUnicode(false)
                .HasColumnName("Type_of_exam");
            entity.Property(e => e.TypeOfLesson)
                .HasMaxLength(50)
                .IsUnicode(false)
                .HasColumnName("Type_of_lesson");
        });

        modelBuilder.Entity<GetAllGroup>(entity =>
        {
            entity
                .HasNoKey()
                .ToView("GetAllGroups");

            entity.Property(e => e.NameDepartment)
                .HasMaxLength(50)
                .IsUnicode(false)
                .HasColumnName("Name_department");
            entity.Property(e => e.NameGroup)
                .HasMaxLength(50)
                .IsUnicode(false)
                .HasColumnName("Name_group");
        });

        modelBuilder.Entity<GetAllTeacher>(entity =>
        {
            entity
                .HasNoKey()
                .ToView("GetAllTeachers");

            entity.Property(e => e.FullName)
                .HasMaxLength(50)
                .IsUnicode(false)
                .HasColumnName("Full_name");
        });

        modelBuilder.Entity<GetTypeExam>(entity =>
        {
            entity
                .HasNoKey()
                .ToView("GetTypeExams");

            entity.Property(e => e.TypeOfExam)
                .HasMaxLength(50)
                .IsUnicode(false)
                .HasColumnName("Type_of_exam");
        });

        modelBuilder.Entity<GetTypeLesson>(entity =>
        {
            entity
                .HasNoKey()
                .ToView("GetTypeLessons");

            entity.Property(e => e.TypeOfLesson)
                .HasMaxLength(50)
                .IsUnicode(false)
                .HasColumnName("Type_of_lesson");
        });

        modelBuilder.Entity<Group>(entity =>
        {
            entity.HasKey(e => e.IdGroup).HasName("PK__Group__8BE8BA1BA749B929");

            entity.ToTable("Group");

            entity.Property(e => e.IdGroup).HasColumnName("id_group");
            entity.Property(e => e.IdDepartment).HasColumnName("id_department");
            entity.Property(e => e.NameOfGroup)
                .HasMaxLength(50)
                .IsUnicode(false)
                .HasColumnName("Name_of_group");

            entity.HasOne(d => d.IdDepartmentNavigation).WithMany(p => p.Groups)
                .HasForeignKey(d => d.IdDepartment)
                .HasConstraintName("FK_Departments_Groups");
        });

        modelBuilder.Entity<Qualification>(entity =>
        {
            entity.HasKey(e => e.IdQualification).HasName("PK__Qualific__DDDA3230D4EFBC89");

            entity.ToTable("Qualification");

            entity.Property(e => e.IdQualification).HasColumnName("id_qualification");
            entity.Property(e => e.NameQualification)
                .HasMaxLength(50)
                .IsUnicode(false)
                .HasColumnName("Name_qualification");
        });

        modelBuilder.Entity<Teacher>(entity =>
        {
            entity.HasKey(e => e.IdTeacher).HasName("PK__Teacher__3BAEF8F97452EB99");

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
                        .OnDelete(DeleteBehavior.ClientSetNull)
                        .HasConstraintName("FK__Teacher_e__id_ex__4D94879B"),
                    l => l.HasOne<Teacher>().WithMany()
                        .HasForeignKey("IdTeacher")
                        .OnDelete(DeleteBehavior.ClientSetNull)
                        .HasConstraintName("FK__Teacher_e__id_te__4CA06362"),
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
            entity.HasKey(e => e.IdTypeOfExam).HasName("PK__Type_of___49A8A9EA085FC3FB");

            entity.ToTable("Type_of_exam");

            entity.Property(e => e.IdTypeOfExam).HasColumnName("id_type_of_exam");
            entity.Property(e => e.TypeOfExam1)
                .HasMaxLength(50)
                .IsUnicode(false)
                .HasColumnName("Type_of_exam");
        });

        modelBuilder.Entity<TypeOfLesson>(entity =>
        {
            entity.HasKey(e => e.IdTypeOfLesson).HasName("PK__Type_of___2BD18CF689BA452A");

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
