using arna.HRMS.Models.Common;
using System.ComponentModel.DataAnnotations;

namespace arna.HRMS.Models.ViewModels;

public class DepartmentViewModel : CommonViewModel
{
    [Required(ErrorMessage = "Department name is required.")]
    [StringLength(100, ErrorMessage = "Department name cannot exceed 100 characters.")]
    [RegularExpression(@"^[a-zA-Z0-9\s&,.-]+$", ErrorMessage = "Department name contains invalid characters.")]
    public string Name { get; set; } = string.Empty;

    [Required(ErrorMessage = "Department code is required.")]
    [StringLength(20, ErrorMessage = "Department code cannot exceed 20 characters.")]
    [RegularExpression(@"^[a-zA-Z0-9_-]+$", ErrorMessage = "Department code can only contain letters, numbers, hyphens, and underscores.")]
    public string Code { get; set; } = string.Empty;

    [StringLength(500, ErrorMessage = "Description cannot exceed 500 characters.")]
    public string? Description { get; set; }

    public int? ParentDepartmentId { get; set; }

    public string? ParentDepartMentName { get; set; }
}
