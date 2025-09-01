using System;
using System.Collections.Generic;

namespace ZabgcExamsDesktop.MVVM.Model.DataBase.Models;

public partial class Group
{
    public int IdGroup { get; set; }

    public int? IdDepartment { get; set; }

    public string? NameOfGroup { get; set; }

    public virtual ICollection<Exam> Exams { get; set; } = new List<Exam>();

    public virtual Department? IdDepartmentNavigation { get; set; }
}
