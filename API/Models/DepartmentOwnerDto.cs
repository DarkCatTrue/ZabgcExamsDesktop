namespace ZabgcExamsDesktop.API.Models
{
    public class DepartmentOwnerDto : BaseDto
    {
        public int IdOwner { get; set; }

        public int IdDepartment { get; set; }

        public string OwnerName { get; set; } = null!;
    }
}
