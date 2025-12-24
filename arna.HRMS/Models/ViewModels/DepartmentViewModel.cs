using arna.HRMS.Models.DTOs;
using System.ComponentModel.DataAnnotations;

namespace arna.HRMS.Models.ViewModels;

public class DepartmentViewModel
{
    public DepartmentDto Department { get; set; }
    public int Id { get; set; }
    [Required(ErrorMessage = "Invalid Detartment Name")]
    public string DepartmentName { get; set; }
    [Required]
    public string Code { get; set; }
    [Required(ErrorMessage = "Invalid Detartment Description")]
    public string Description { get; set; }
    public int? ParentDepartmentId { get; set; }
    [Display(Name = "Status")]
    public bool IsActive { get; set; }
}
