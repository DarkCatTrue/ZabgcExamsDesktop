using System;
using System.Collections.Generic;

namespace ZabgcExamsDesktop.MVVM.Model.DataBase.Models;

public partial class Manager
{
    public int IdManager { get; set; }

    public string Post { get; set; } = null!;

    public string FullName { get; set; } = null!;
}
