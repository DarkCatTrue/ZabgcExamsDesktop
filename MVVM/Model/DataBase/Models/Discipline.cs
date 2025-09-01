using System;
using System.Collections.Generic;

namespace ZabgcExamsDesktop.MVVM.Model.DataBase.Models;

public partial class Discipline
{
    public int IdDiscipline { get; set; }

    public string? NameDiscipline { get; set; }

    public virtual ICollection<Exam> Exams { get; set; } = new List<Exam>();
}
