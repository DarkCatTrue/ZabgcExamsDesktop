namespace ZabgcExamsDesktop.API.Models
{
    public abstract class BaseDto
    {
        public bool IsNew { get; set; }
        public bool IsEditing { get; set; }
        public bool IsPersisted { get; set; }
    }
}
