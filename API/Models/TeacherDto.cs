namespace ZabgcExamsDesktop.API.Models
{
    public class TeacherDto : BaseDto
    {
        public int IdTeacher { get; set; }
        public string FullName { get; set; } = string.Empty;
    }
}
