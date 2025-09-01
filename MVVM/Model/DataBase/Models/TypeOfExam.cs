using System;
using System.Collections.Generic;

namespace ZabgcExamsDesktop.MVVM.Model.DataBase.Models;

public partial class TypeOfExam
{
    public int IdTypeOfExam { get; set; }

    public string? TypeOfExam1 { get; set; }

    public virtual ICollection<Exam> Exams { get; set; } = new List<Exam>();
}
