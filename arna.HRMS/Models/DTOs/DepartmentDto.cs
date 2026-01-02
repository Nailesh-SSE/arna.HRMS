using arna.HRMS.Models.Common;
using System.ComponentModel.DataAnnotations;

namespace arna.HRMS.Models.DTOs;

public class DepartmentDto : CommonDto
{
    [Required(ErrorMessage = "Invalid Detartment Name")]
    public string Name { get; set; }

    [Required]
    public string Code { get; set; }

    [Required(ErrorMessage = "Invalid Detartment Description")]
    public string Description { get; set; }

    public int? ParentDepartmentId { get; set; }
}