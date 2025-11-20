namespace ZabgcExamsDesktop.API.Models
{
    public class CreateExamDto
    {
        public int IdGroup { get; set; }
        public int IdDiscipline { get; set; }
        public int IdTypeOfLesson { get; set; }
        public int IdTypeOfExam { get; set; }
        public DateTime DateEvent { get; set; }
        public int IdAudience { get; set; }
        public List<int> IdTeachers { get; set; } = new();
    }
}
