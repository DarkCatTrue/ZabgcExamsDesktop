using System;
using System.Collections.Generic;

namespace ZabgcExamsDesktop.MVVM.Model.DataBase.Models;

public partial class Teacher
{
    public int IdTeacher { get; set; }

    public string? FullName { get; set; }

    public virtual ICollection<Exam> IdExams { get; set; } = new List<Exam>();
}
