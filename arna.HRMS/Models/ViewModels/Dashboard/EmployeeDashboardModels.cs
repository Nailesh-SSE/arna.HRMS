using System;
using arna.HRMS.Models.Enums;

namespace arna.HRMS.Models.ViewModels.Dashboard
{
    public class CombinedRequestItem
    {
        public int Id { get; set; }
        public string RequestType { get; set; } = "Leave Request";
        public string RequestName { get; set; } = "";
        public string DateRangeDisplay { get; set; } = "";
        public string CreatedOnDisplay { get; set; } = "";
        public Status StatusId { get; set; } = Status.Pending;
        public DateTime SortDate { get; set; }
        public string LinkUrl { get; set; } = "/emp-leave-management";
    }

    public class WeeklyDayAttendanceItem
    {
        public DayOfWeek DayOfWeek { get; set; }
        public string DayName { get; set; } = "";
        public string DateFormatted { get; set; } = "";
        public DateTime Date { get; set; }
        public string DurationDisplay { get; set; } = "--";
        public string StatusLabel { get; set; } = "Upcoming";
        public string StatusClass { get; set; } = "day-upcoming";
        public bool IsToday { get; set; }
        public bool IsWeekend { get; set; }
        public bool IsLeave { get; set; }
        public string LeaveType { get; set; } = "";
    }
}
