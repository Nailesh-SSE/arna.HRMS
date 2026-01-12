namespace arna.HRMS.Components.Common.Tables;

public static class EmployeeTableHeaders
{
    public static IReadOnlyList<TableHeader> GetHeaders(bool showActions)
    {
        var headers = new List<TableHeader>
        {
            new() { Title = "#" },
            new() { Title = "Employee" },
            new() { Title = "Email" },
            new() { Title = "Position" },
            new() { Title = "Date Of Birth" },
            new() { Title = "Dept Code" },
            new() { Title = "Manager" },
            new() { Title = "Status" }
        };

        if (showActions)
        {
            headers.Add(new TableHeader
            {
                Title = "Actions",
                CssClass = "text-center"
            });
        }

        return headers;
    }
}
