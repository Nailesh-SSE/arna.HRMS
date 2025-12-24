using Microsoft.AspNetCore.Identity;

namespace arna.HRMS.Infrastructure.Data.Identity;

public class ApplicationRole : IdentityRole<int>
{
    public string Description { get; set; }
}
