using System.ComponentModel.DataAnnotations;

namespace arna.HRMS.Models.DTOs;

public class DepartmentDto
{
    public int Id { get; set; }
    [Required(ErrorMessage = "Invalid Detartment Name")]
    public string Name { get; set; }
    [Required]
    public string Code { get; set; }
    [Required(ErrorMessage = "Invalid Detartment Description")]
    public string Description { get; set; }
    public int? ParentDepartmentId { get; set; }
    public string? parentDepartMentName { get; set; }
    [Display(Name = "Status")]
    public bool IsActive { get; set; }

    // Optional: Auditing fields if needed in the response
    public DateTime CreatedAt { get; set; }

}
