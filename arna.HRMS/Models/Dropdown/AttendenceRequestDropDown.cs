using arna.HRMS.Components.lib.components.ui.Model;

namespace arna.HRMS.Models.Dropdown;

public static class AttendenceRequestDropDown
{
    public static List<SelectOption> ReasonOption => new()
    {
        new SelectOption { Value = "1", Text = "Forgot To ClockOut" },
        new SelectOption { Value = "2", Text = "Forgot To ClockIn" },
        new SelectOption { Value = "3", Text = "Early ClockOut" },
        new SelectOption { Value = "4", Text = "Late ClockIn" },
        new SelectOption { Value = "5", Text = "Client Visit" },
        new SelectOption { Value = "6", Text = "Work From Home" },
        new SelectOption { Value = "7", Text = "Technical Issue" },
        new SelectOption { Value = "8", Text = "Comp Off Request" }
    };

    public static List<SelectOption> LocationOption => new()
    {
        new SelectOption { Value = "1", Text = "Office" },
        new SelectOption { Value = "2", Text = "Remote" },
        new SelectOption { Value = "3", Text = "ClientSite" }
    };
}
