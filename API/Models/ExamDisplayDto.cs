using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ZabgcExamsDesktop.API.Models
{
    public class ExamDisplayDto
    {
        public int IdExam { get; set; }
        public int IdGroup { get; set; }
        public int IdDiscipline { get; set; }
        public int IdTypeOfLesson { get; set; }
        public int IdTypeOfExam { get; set; }
        public DateTime DateEvent { get; set; }
        public int IdAudience { get; set; }
        public List<int> IdTeachers { get; set; } = new();

        public string DepartmentName { get; set; } = string.Empty;
        public string GroupName { get; set; } = string.Empty;
        public string TypeOfLessonName { get; set; } = string.Empty;
        public string TypeOfExamName { get; set; } = string.Empty;
        public string QualificationName { get; set; } = string.Empty;
        public string DisciplineName { get; set; } = string.Empty;
        public string TeachersDisplay { get; set; } = string.Empty;
        public string AudienceNumber { get; set; } = string.Empty;
    }
}
