using arna.HRMS.Models.ViewModels.Attendance;

namespace arna.HRMS.Helper.Attendance;

public static class MonthlyAttendanceBuilder
{
    public static List<MonthlyAttendanceViewModel> Build(
        int year,
        int month,
        int employeeId,
        List<MonthlyAttendanceViewModel> apiData)
    {
        var lookup = apiData.ToDictionary(x => x.Date);
        var result = new List<MonthlyAttendanceViewModel>();

        int daysInMonth = DateTime.DaysInMonth(year, month);

        for (int day = 1; day <= daysInMonth; day++)
        {
            var date = new DateOnly(year, month, day);

            if (lookup.TryGetValue(date, out var record))
            {
                // ✅ Use API record if exists
                result.Add(record);
            }
            else
            {
                // ✅ Blank record (future or not-yet-recorded)
                result.Add(new MonthlyAttendanceViewModel
                {
                    EmployeeId = employeeId,
                    Date = date,
                    Day = date.DayOfWeek.ToString(),
                    ClockIn = null,
                    ClockOut = null,
                    TotalHours = TimeSpan.FromHours(0),
                    Status = string.Empty
                });
            }
        }

        return result;
    }
}
