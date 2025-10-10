using System;
using System.Collections.Generic;

namespace ZabgcExamsDesktop.MVVM.Model.DataBase.Models;

public partial class DepartmentOwner
{
    public int IdOwner { get; set; }

    public int IdDepartment { get; set; }

    public string OwnerName { get; set; } = null!;

    public virtual Department IdDepartmentNavigation { get; set; } = null!;
}
