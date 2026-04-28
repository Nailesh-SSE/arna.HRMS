using arna.HRMS.Core.Common.Base;

namespace arna.HRMS.Core.DTOs;

public class DepartmentDto : BaseEntity
{
    public string Name { get; set; }
    public string Code { get; set; }
    public string Description { get; set; }
    public int? ParentDepartmentId { get; set; }
    public string? ParentDepartMentName { get; set; }
}