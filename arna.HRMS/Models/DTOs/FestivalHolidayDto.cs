using arna.HRMS.Models.Common;
using System.ComponentModel.DataAnnotations;

namespace arna.HRMS.Models.DTOs;

public class FestivalHolidayDto : CommonDto
{
    [Required(ErrorMessage = "Date is required")]
    public DateTime Date { get; set; }
    [Required(ErrorMessage ="Festival name is required")]
    public string FestivalName { get; set; }
    public string? Description { get; set; }
}
