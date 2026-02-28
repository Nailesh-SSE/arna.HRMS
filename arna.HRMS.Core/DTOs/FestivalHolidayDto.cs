using arna.HRMS.Core.Common.Base;

namespace arna.HRMS.Core.DTOs;

public class FestivalHolidayDto : BaseEntity
{
    public DateTime Date { get; set; }
    public string FestivalName { get; set; }
    public string? Description { get; set; }
    public string DayOfWeek => Date.DayOfWeek.ToString();
}
