using arna.HRMS.Models.DTOs;

namespace arna.HRMS.Helpers.Attendance;

public static class MonthlyAttendanceBuilder
{
    public static List<MonthlyAttendanceDto> Build(
        int year,
        int month,
        int employeeId,
        List<MonthlyAttendanceDto> apiData)
    {
        var lookup = apiData.ToDictionary(x => x.Date);
        var result = new List<MonthlyAttendanceDto>();

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
                result.Add(new MonthlyAttendanceDto
                {
                    EmployeeId = employeeId,
                    Date = date,
                    Day = date.DayOfWeek.ToString(),
                    ClockIn = null,
                    ClockOut = null,
                    TotalHours = 0,
                    Status = string.Empty
                });
            }
        }

        return result;
    }
}
