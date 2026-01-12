namespace arna.HRMS.Models.Common.Table;

public class TableColumn<T>
{
    public string Header { get; set; } = "";
    public Func<T, object?> Value { get; set; } = default!;
    public Func<T, string>? CssClassFunc { get; set; }
    public string? CssClass { get; set; }
}
