using arna.HRMS.Core.Common.Base;

namespace arna.HRMS.Core.Entities;

public class Role : BaseEntity
{
    public string Name { get; set; } = string.Empty;
    public string? Description { get; set; } = string.Empty;
}
