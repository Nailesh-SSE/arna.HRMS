namespace arna.HRMS.Components.Common.Tables;

public class TableHeader
{
    public string Title { get; set; } = string.Empty;
    public string? CssClass { get; set; }
    public bool Hidden { get; set; } = false;
    public bool NoWrap { get; set; } = true;
}
