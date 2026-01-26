using arna.HRMS.Core.Common;

namespace arna.HRMS.Core.DTOs;

public class RoleDto : BaseEntity
{
    public string Name { get; set; } = string.Empty;
    public string? Description { get; set; } = string.Empty;
}
