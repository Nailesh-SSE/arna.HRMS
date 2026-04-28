using arna.HRMS.Models.Common;

namespace arna.HRMS.Models.ViewModels;

public class RoleViewModel : CommonViewModel
{
    public string Name { get; set; } = string.Empty;
    public string? Description { get; set; } = string.Empty;
}
