using arna.HRMS.Core.Common;
using System.ComponentModel.DataAnnotations;

namespace arna.HRMS.Core.DTOs;

public class FestivalHolidayDto : BaseEntity
{
    [Required(ErrorMessage = "Date is required")]
    public DateTime Date { get; set; }
    public string DayOfWeek => Date.DayOfWeek.ToString();
    [Required(ErrorMessage ="Festival name is required")]
    public string FestivalName { get; set; }
    public string? Description { get; set; }
}
