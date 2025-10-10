using System;
using System.Collections.Generic;

namespace ZabgcExamsDesktop.MVVM.Model.DataBase.Models;

public partial class Audience
{
    public int IdAudience { get; set; }

    public int? NumberAudience { get; set; }

    public virtual ICollection<Exam> Exams { get; set; } = new List<Exam>();
}
