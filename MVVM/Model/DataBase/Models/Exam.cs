using System;
using System.Collections.Generic;
using System.ComponentModel;

namespace ZabgcExamsDesktop.MVVM.Model.DataBase.Models;

public partial class Exam
{

    public event PropertyChangedEventHandler PropertyChanged;

    protected virtual void OnPropertyChanged(string propertyName)
    {
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
    }

    public int IdExam { get; set; }

    public int IdGroup { get; set; }

    public int IdDiscipline { get; set; }

    public int IdTypeOfLesson { get; set; }

    public int IdTypeOfExam { get; set; }

    public int IdQualification { get; set; }

    public DateTime DateEvent { get; set; }

    public int IdAudience { get; set; }

    public virtual Audience IdAudienceNavigation { get; set; } = null!;

    public virtual Group IdGroupNavigation { get; set; } = null!;

    public virtual Qualification IdQualificationNavigation { get; set; } = null!;

    public virtual TypeOfExam IdTypeOfExamNavigation { get; set; } = null!;

    public virtual TypeOfLesson IdTypeOfLessonNavigation { get; set; } = null!;

    public virtual Discipline IdDisciplineNavigation { get; set; } = null!;

    public virtual ICollection<Teacher> IdTeachers { get; set; } = new List<Teacher>();
}
