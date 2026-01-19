using System.ComponentModel;

namespace arna.HRMS.Core.Common;

public abstract class BaseEntity : IBaseEntity
{
    public int Id { get; set; }
    public DateTime CreatedBy { get; set; } = DateTime.Now;
    public DateTime UpdatedBy { get; set; } = DateTime.Now;
    [DefaultValue(true)]
    public bool IsActive { get; set; } = true;
    public bool IsDeleted { get; set; } = false;
}
