using System.ComponentModel;

namespace arna.HRMS.Core.Common.Base;

public abstract class BaseEntity : IBaseEntity
{
    public int Id { get; set; }
    public DateTime CreatedOn { get; set; } = DateTime.Now;
    public DateTime UpdatedOn { get; set; } = DateTime.Now;
    [DefaultValue(true)]
    public bool IsActive { get; set; } = true;
    public bool IsDeleted { get; set; } = false;
}
