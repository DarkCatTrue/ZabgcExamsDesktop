using System;
using System.Collections.Generic;

namespace ZabgcExamsDesktop.MVVM.Model.DataBase.Models;

public partial class Exam
{
    public int IdExam { get; set; }

    public int? IdGroup { get; set; }

    public int? IdDiscipline { get; set; }

    public int? IdTypeOfLesson { get; set; }

    public int? IdTypeOfExam { get; set; }

    public int? IdQualification { get; set; }

    public DateTime? DateEvent { get; set; }

    public virtual Discipline? IdDisciplineNavigation { get; set; }

    public virtual Group? IdGroupNavigation { get; set; }

    public virtual Qualification? IdQualificationNavigation { get; set; }

    public virtual TypeOfExam? IdTypeOfExamNavigation { get; set; }

    public virtual TypeOfLesson? IdTypeOfLessonNavigation { get; set; }

    public virtual ICollection<Audience> IdAudiences { get; set; } = new List<Audience>();

    public virtual ICollection<Teacher> IdTeachers { get; set; } = new List<Teacher>();
}
