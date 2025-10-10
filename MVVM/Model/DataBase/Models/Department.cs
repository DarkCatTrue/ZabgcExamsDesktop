using System;
using System.Collections.Generic;

namespace ZabgcExamsDesktop.MVVM.Model.DataBase.Models;

public partial class Department
{
    public int IdDepartment { get; set; }

    public string? NameOfDepartment { get; set; }

    public virtual ICollection<DepartmentOwner> DepartmentOwners { get; set; } = new List<DepartmentOwner>();

    public virtual ICollection<Group> Groups { get; set; } = new List<Group>();
}
