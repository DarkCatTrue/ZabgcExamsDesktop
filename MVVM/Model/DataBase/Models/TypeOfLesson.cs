using System;
using System.Collections.Generic;

namespace ZabgcExamsDesktop.MVVM.Model.DataBase.Models;

public partial class TypeOfLesson
{
    public int IdTypeOfLesson { get; set; }

    public string? TypeOfLesson1 { get; set; }

    public virtual ICollection<Exam> Exams { get; set; } = new List<Exam>();
}
