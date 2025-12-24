using System.ComponentModel.DataAnnotations;

namespace arna.HRMS.Core.DTOs.Requests;

public class UpdateDepartmentRequest
{
    public int Id { get; set; }
    [Required(ErrorMessage = "Invalid Detartment Name")]
    public string Name { get; set; }
    [Required]
    public string Code { get; set; }
    [Required(ErrorMessage = "Invalid Detartment Description")]
    public string Description { get; set; }
    public int? ParentDepartmentId { get; set; }
    [Display(Name = "Status")]
    public bool IsActive { get; set; }
}
