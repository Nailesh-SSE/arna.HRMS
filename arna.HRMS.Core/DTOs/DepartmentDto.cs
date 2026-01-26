using arna.HRMS.Core.Common;
using System.ComponentModel.DataAnnotations;

namespace arna.HRMS.Core.DTOs;

public class DepartmentDto : BaseEntity
{
    [Required(ErrorMessage = "Invalid Detartment Name")]
    public string Name { get; set; }

    [Required]
    public string Code { get; set; }

    [Required(ErrorMessage = "Invalid Detartment Description")]
    public string Description { get; set; }

    public int? ParentDepartmentId { get; set; }

    public string? ParentDepartMentName { get; set; }
}