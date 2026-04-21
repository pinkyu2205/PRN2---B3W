namespace Domain.Common
{
    public abstract class BaseFullEntity : IBaseEntity, IAuditable
    {
        public int Id { get; set; }
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public string CreatedBy { get; set; } = "system";
        public DateTime ModifiedAt { get; set; } = DateTime.UtcNow;
        public string ModifiedBy { get; set; } = "system";
        public bool IsDeleted { get; set; } = false;
    }
}
