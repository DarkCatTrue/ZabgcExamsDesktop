using System;
using System.Collections.Generic;

namespace ZabgcExamsDesktop.MVVM.Model.DataBase.Models;

public partial class Qualification
{
    public int IdQualification { get; set; }

    public string? NameQualification { get; set; }

    public virtual ICollection<Exam> Exams { get; set; } = new List<Exam>();
}
