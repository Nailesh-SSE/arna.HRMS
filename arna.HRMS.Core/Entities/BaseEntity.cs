using System.ComponentModel;

namespace arna.HRMS.Core.Entities;

public abstract class BaseEntity : IBaseEntity
{
    public int Id { get; set; }
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;
    [DefaultValue(true)]
    public bool IsActive { get; set; } = true;
    public bool IsDeleted { get; set; } = false;
}
