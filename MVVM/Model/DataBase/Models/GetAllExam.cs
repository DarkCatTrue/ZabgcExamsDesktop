using System;
using System.Collections.Generic;

namespace ZabgcExamsDesktop.MVVM.Model.DataBase.Models;

public partial class GetAllExam
{
    public int ExamId { get; set; }

    public string? Department { get; set; }

    public string? GroupName { get; set; }

    public string? NameDiscipline { get; set; }

    public string? TeacherName { get; set; }

    public int? NumberAudience { get; set; }

    public string? TypeOfLesson { get; set; }

    public string? TypeOfExam { get; set; }
}
