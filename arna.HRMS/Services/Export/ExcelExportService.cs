using ClosedXML.Excel;
using arna.HRMS.Models.Enums;
using arna.HRMS.Models.ViewModels;
using arna.HRMS.Models.ViewModels.Attendance;

namespace arna.HRMS.Services.Export;

public interface IExcelExportService
{
    byte[] ExportAttendanceReport(
        EmployeeViewModel employee,
        List<AttendanceReportViewModel> records,
        string periodTitle,
        double attendanceRate,
        double effectiveDays,
        int totalWorkingDays,
        int presentCount,
        int lateCount,
        int halfDayCount,
        int absentCount,
        int onLeaveCount);
}

public class ExcelExportService : IExcelExportService
{
    public byte[] ExportAttendanceReport(
        EmployeeViewModel employee,
        List<AttendanceReportViewModel> records,
        string periodTitle,
        double attendanceRate,
        double effectiveDays,
        int totalWorkingDays,
        int presentCount,
        int lateCount,
        int halfDayCount,
        int absentCount,
        int onLeaveCount)
    {
        using var workbook = new XLWorkbook();
        var ws = workbook.Worksheets.Add("Attendance Report");
        ws.ShowGridLines = true;

        // 1. Report Title Banner
        ws.Range("B2:L2").Merge();
        var titleCell = ws.Cell("B2");
        titleCell.Value = "ARNA HRMS - EMPLOYEE ATTENDANCE REPORT";
        titleCell.Style.Font.Bold = true;
        titleCell.Style.Font.FontSize = 16;
        titleCell.Style.Font.FontColor = XLColor.White;
        titleCell.Style.Fill.BackgroundColor = XLColor.FromHtml("#1E3A8A"); // Navy blue
        titleCell.Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Center;
        titleCell.Style.Alignment.Vertical = XLAlignmentVerticalValues.Center;
        ws.Row(2).Height = 35;

        // Subtitle
        ws.Range("B3:L3").Merge();
        var subtitleCell = ws.Cell("B3");
        subtitleCell.Value = $"Period: {periodTitle}  |  Generated On: {DateTime.Now:dd MMM yyyy, hh:mm tt}";
        subtitleCell.Style.Font.FontSize = 10;
        subtitleCell.Style.Font.FontColor = XLColor.FromHtml("#475569");
        subtitleCell.Style.Fill.BackgroundColor = XLColor.FromHtml("#F1F5F9");
        subtitleCell.Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Center;
        subtitleCell.Style.Alignment.Vertical = XLAlignmentVerticalValues.Center;
        ws.Row(3).Height = 22;

        // 2. Employee Profile Info Card (Rows 5-6)
        ws.Cell("B5").Value = "Employee Name:";
        ws.Cell("B5").Style.Font.Bold = true;
        ws.Cell("B5").Style.Font.FontColor = XLColor.FromHtml("#334155");
        ws.Cell("C5").Value = employee.FullName ?? "N/A";
        ws.Cell("C5").Style.Font.Bold = true;

        ws.Cell("E5").Value = "Employee ID:";
        ws.Cell("E5").Style.Font.Bold = true;
        ws.Cell("E5").Style.Font.FontColor = XLColor.FromHtml("#334155");
        ws.Cell("F5").Value = employee.EmployeeNumber ?? "N/A";

        ws.Cell("H5").Value = "Designation:";
        ws.Cell("H5").Style.Font.Bold = true;
        ws.Cell("H5").Style.Font.FontColor = XLColor.FromHtml("#334155");
        ws.Cell("I5").Value = employee.Designation?.ToString() ?? "N/A";

        ws.Cell("B6").Value = "Department:";
        ws.Cell("B6").Style.Font.Bold = true;
        ws.Cell("B6").Style.Font.FontColor = XLColor.FromHtml("#334155");
        ws.Cell("C6").Value = employee.DepartmentName ?? "N/A";

        ws.Cell("E6").Value = "Attendance Rate:";
        ws.Cell("E6").Style.Font.Bold = true;
        ws.Cell("E6").Style.Font.FontColor = XLColor.FromHtml("#334155");
        ws.Cell("F6").Value = $"{attendanceRate}%";
        ws.Cell("F6").Style.Font.Bold = true;
        ws.Cell("F6").Style.Font.FontColor = attendanceRate >= 80 ? XLColor.FromHtml("#16A34A") : XLColor.FromHtml("#DC2626");

        ws.Cell("H6").Value = "Effective Present:";
        ws.Cell("H6").Style.Font.Bold = true;
        ws.Cell("H6").Style.Font.FontColor = XLColor.FromHtml("#334155");
        ws.Cell("I6").Value = $"{effectiveDays} / {records.Count} Days";

        var infoRange = ws.Range("B5:J6");
        infoRange.Style.Border.OutsideBorder = XLBorderStyleValues.Thin;
        infoRange.Style.Border.OutsideBorderColor = XLColor.FromHtml("#CBD5E1");
        infoRange.Style.Fill.BackgroundColor = XLColor.FromHtml("#F8FAFC");

        // 3. KPI Metrics Badges (Row 8)
        void StyleMetricPill(string cell, string label, string val, string bgHex, string textHex)
        {
            var c = ws.Cell(cell);
            c.Value = $"{label}: {val}";
            c.Style.Font.Bold = true;
            c.Style.Font.FontSize = 10;
            c.Style.Font.FontColor = XLColor.FromHtml(textHex);
            c.Style.Fill.BackgroundColor = XLColor.FromHtml(bgHex);
            c.Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Center;
            c.Style.Alignment.Vertical = XLAlignmentVerticalValues.Center;
            c.Style.Border.OutsideBorder = XLBorderStyleValues.Thin;
            c.Style.Border.OutsideBorderColor = XLColor.FromHtml(textHex);
        }

        StyleMetricPill("B8", "Total Days", records.Count.ToString(), "#EFF6FF", "#1D4ED8");
        StyleMetricPill("C8", "Present", presentCount.ToString(), "#DCFCE7", "#15803D");
        StyleMetricPill("D8", "Late", lateCount.ToString(), "#FEF9C3", "#A16207");
        StyleMetricPill("E8", "Half Day", halfDayCount.ToString(), "#CFFAFE", "#0E7490");
        StyleMetricPill("F8", "Absent", absentCount.ToString(), "#FEE2E2", "#B91C1C");
        StyleMetricPill("G8", "On Leave", onLeaveCount.ToString(), "#F3E8FF", "#7E22CE");
        StyleMetricPill("H8", "Working Days", totalWorkingDays.ToString(), "#F1F5F9", "#475569");
        ws.Row(8).Height = 25;

        // 4. Main Data Table Header (Row 10)
        string[] headers =
        {
            "#", "Date", "Day", "Status", "Clock In", "Clock Out",
            "Working Hours", "Break", "Total Hours", "Location / Map", "Device"
        };

        int startRow = 10;
        int colIndex = 2; // Column B

        for (int i = 0; i < headers.Length; i++)
        {
            var cell = ws.Cell(startRow, colIndex + i);
            cell.Value = headers[i];
            cell.Style.Font.Bold = true;
            cell.Style.Font.FontSize = 10;
            cell.Style.Font.FontColor = XLColor.White;
            cell.Style.Fill.BackgroundColor = XLColor.FromHtml("#1E293B"); // Slate dark
            cell.Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Center;
            cell.Style.Alignment.Vertical = XLAlignmentVerticalValues.Center;
            cell.Style.Border.OutsideBorder = XLBorderStyleValues.Thin;
            cell.Style.Border.OutsideBorderColor = XLColor.FromHtml("#475569");
        }
        ws.Row(startRow).Height = 26;

        // 5. Data Rows
        int currentRow = startRow + 1;
        int itemIndex = 1;

        foreach (var r in records.OrderBy(x => x.AttendanceDate))
        {
            ws.Row(currentRow).Height = 22;

            // Col B: #
            ws.Cell(currentRow, 2).Value = itemIndex++;
            ws.Cell(currentRow, 2).Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Center;

            // Col C: Date
            ws.Cell(currentRow, 3).Value = r.AttendanceDate.ToString("dd MMM yyyy");
            ws.Cell(currentRow, 3).Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Center;

            // Col D: Day
            ws.Cell(currentRow, 4).Value = r.Day ?? r.AttendanceDate.ToString("ddd");
            ws.Cell(currentRow, 4).Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Center;

            // Col E: Status
            var statusCell = ws.Cell(currentRow, 5);
            var statusText = r.AttendanceStatus?.ToString() ?? "—";
            statusCell.Value = statusText;
            statusCell.Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Center;
            statusCell.Style.Font.Bold = true;

            switch (r.AttendanceStatus)
            {
                case AttendanceStatus.Present:
                    statusCell.Style.Fill.BackgroundColor = XLColor.FromHtml("#DCFCE7");
                    statusCell.Style.Font.FontColor = XLColor.FromHtml("#166534");
                    break;
                case AttendanceStatus.Absent:
                    statusCell.Style.Fill.BackgroundColor = XLColor.FromHtml("#FEE2E2");
                    statusCell.Style.Font.FontColor = XLColor.FromHtml("#991B1B");
                    break;
                case AttendanceStatus.Late:
                    statusCell.Style.Fill.BackgroundColor = XLColor.FromHtml("#FEF9C3");
                    statusCell.Style.Font.FontColor = XLColor.FromHtml("#854D0E");
                    break;
                case AttendanceStatus.HalfDay:
                    statusCell.Style.Fill.BackgroundColor = XLColor.FromHtml("#CFFAFE");
                    statusCell.Style.Font.FontColor = XLColor.FromHtml("#155E75");
                    break;
                case AttendanceStatus.OnLeave:
                    statusCell.Style.Fill.BackgroundColor = XLColor.FromHtml("#F3E8FF");
                    statusCell.Style.Font.FontColor = XLColor.FromHtml("#6B21A8");
                    break;
                case AttendanceStatus.Holiday:
                    statusCell.Style.Fill.BackgroundColor = XLColor.FromHtml("#DBEAFE");
                    statusCell.Style.Font.FontColor = XLColor.FromHtml("#1E40AF");
                    break;
                default:
                    statusCell.Style.Font.FontColor = XLColor.FromHtml("#64748B");
                    break;
            }

            // Col F: Clock In
            ws.Cell(currentRow, 6).Value = r.ClockIn.HasValue
                ? DateTime.Today.Add(r.ClockIn.Value).ToString("hh:mm tt")
                : "--";
            ws.Cell(currentRow, 6).Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Center;

            // Col G: Clock Out
            ws.Cell(currentRow, 7).Value = r.ClockOut.HasValue
                ? DateTime.Today.Add(r.ClockOut.Value).ToString("hh:mm tt")
                : "--";
            ws.Cell(currentRow, 7).Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Center;

            // Col H: Working Hours
            ws.Cell(currentRow, 8).Value = r.WorkingHours.HasValue ? r.WorkingHours.Value.ToString(@"hh\:mm") : "--";
            ws.Cell(currentRow, 8).Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Center;

            // Col I: Break
            ws.Cell(currentRow, 9).Value = r.BreakDuration.HasValue ? r.BreakDuration.Value.ToString(@"hh\:mm") : "--";
            ws.Cell(currentRow, 9).Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Center;

            // Col J: Total Hours
            ws.Cell(currentRow, 10).Value = r.TotalHours.HasValue ? r.TotalHours.Value.ToString(@"hh\:mm") : "--";
            ws.Cell(currentRow, 10).Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Center;

            // Col K: Location / Map Hyperlink
            var mapCell = ws.Cell(currentRow, 11);
            if (r.Latitude.HasValue && r.Longitude.HasValue &&
                (Math.Abs(r.Latitude.Value) > 0.0001 || Math.Abs(r.Longitude.Value) > 0.0001))
            {
                var lat = r.Latitude.Value.ToString(System.Globalization.CultureInfo.InvariantCulture);
                var lon = r.Longitude.Value.ToString(System.Globalization.CultureInfo.InvariantCulture);
                mapCell.FormulaA1 = $"HYPERLINK(\"https://www.google.com/maps?q={lat},{lon}\", \"View Map\")";
                mapCell.Style.Font.FontColor = XLColor.FromHtml("#2563EB");
                mapCell.Style.Font.Underline = XLFontUnderlineValues.Single;
                mapCell.Style.Font.Bold = true;
            }
            else
            {
                mapCell.Value = "--";
                mapCell.Style.Font.FontColor = XLColor.FromHtml("#94A3B8");
            }
            mapCell.Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Center;

            // Col L: Device
            var deviceNames = r.Devices != null && r.Devices.Any()
                ? string.Join(", ", r.Devices.Select(d => d switch
                {
                    DeviceType.Mobile => "Mobile",
                    DeviceType.Tablet => "Tablet",
                    DeviceType.LaptopDesktop => "Desktop",
                    _ => "Unknown"
                }))
                : "--";
            ws.Cell(currentRow, 12).Value = deviceNames;
            ws.Cell(currentRow, 12).Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Center;

            // Row zebra striping & border
            var rowRange = ws.Range(currentRow, 2, currentRow, 12);
            rowRange.Style.Border.OutsideBorder = XLBorderStyleValues.Thin;
            rowRange.Style.Border.InsideBorder = XLBorderStyleValues.Thin;
            rowRange.Style.Border.OutsideBorderColor = XLColor.FromHtml("#E2E8F0");
            rowRange.Style.Border.InsideBorderColor = XLColor.FromHtml("#E2E8F0");

            if (itemIndex % 2 == 0 && r.AttendanceStatus == AttendanceStatus.Present)
            {
                rowRange.Style.Fill.BackgroundColor = XLColor.FromHtml("#F8FAFC");
            }

            currentRow++;
        }

        // Auto-fit columns with safety margin
        for (int c = 2; c <= 12; c++)
        {
            ws.Column(c).AdjustToContents();
            if (ws.Column(c).Width < 12)
                ws.Column(c).Width = 12;
        }
        ws.Column(2).Width = 6;  // # column
        ws.Column(3).Width = 15; // Date
        ws.Column(5).Width = 14; // Status
        ws.Column(11).Width = 16; // Location

        using var ms = new MemoryStream();
        workbook.SaveAs(ms);
        return ms.ToArray();
    }
}
