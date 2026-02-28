using arna.HRMS.Models.Enums;

namespace arna.HRMS.Helper.Navigation;

public class NavigationHelper
{
    // Initialized once – no repeated allocations 
    private static readonly IReadOnlyList<NavMenuItem> _menuItems = new List<NavMenuItem>
    {
        new()
        {
            Id = "dashboard",
            Label = "Dashboard",
            Icon = "bi bi-grid-1x2-fill",
            Section = "Main",
            Route = "/",
            ExactMatch = true,
            AllowedRoles = new() { UserRole.SuperAdmin, UserRole.Admin, UserRole.HR, UserRole.Manager, UserRole.Employee }
        },
        new()
        {
            Id = "employees",
            Label = "Employees",
            Icon = "bi bi-people-fill",
            Section = "Main",
            Route = "/employee",
            AllowedRoles = new() { UserRole.SuperAdmin, UserRole.Admin, UserRole.HR, UserRole.Manager, UserRole.Employee }
        },
        new()
        {
            Id = "attendance",
            Label = "Attendance",
            Icon = "bi bi-calendar2-check-fill",
            Section = "Main",
            Route = "/attendance",
            AllowedRoles = new() { UserRole.SuperAdmin, UserRole.Admin, UserRole.HR, UserRole.Manager, UserRole.Employee }
        },
        new()
        {
            Id = "leave-management",
            Label = "Leave Management",
            Icon = "bi bi-calendar-x-fill",
            Section = "Leave",
            Route = "/leave-management",
            AllowedRoles = new() { UserRole.SuperAdmin, UserRole.Admin, UserRole.HR, UserRole.Employee }
        },
        new()
        {
            Id = "department",
            Label = "Department",
            Icon = "bi bi-building-fill",
            Section = "Administration",
            Route = "/department",
            AllowedRoles = new() { UserRole.SuperAdmin, UserRole.Admin, UserRole.HR }
        },
        new()
        {
            Id = "role",
            Label = "Role",
            Icon = "bi bi-shield-fill-check",
            Section = "Administration",
            Route = "/role",
            AllowedRoles = new() { UserRole.SuperAdmin, UserRole.Admin, UserRole.HR }
        },
        new()
        {
            Id = "users",
            Label = "Users",
            Icon = "bi bi-person-fill-gear",
            Section = "Administration",
            Route = "/user",
            AllowedRoles = new() { UserRole.SuperAdmin, UserRole.Admin, UserRole.HR }
        },
        new()
        {
            Id = "profile",
            Label = "My Profile",
            Icon = "bi bi-person-badge-fill",
            Section = "Account",
            Route = "/profile",
            AllowedRoles = new() { UserRole.SuperAdmin, UserRole.Admin, UserRole.HR, UserRole.Manager, UserRole.Employee }
        }
    };

    public static IReadOnlyList<NavMenuItem> GetMenuItems()
        => _menuItems;

    public static IReadOnlyList<NavMenuItem> GetMenuItemsByRole(UserRole role)
    {
        return _menuItems
            .Where(item => IsAccessible(item, role))
            .ToList();
    }

    private static bool IsAccessible(NavMenuItem item, UserRole role)
    {
        if (item.VisibilityCondition != null && !item.VisibilityCondition(role))
            return false;

        if (item.ExcludedRoles.Contains(role))
            return false;

        if (item.AllowedRoles.Count > 0)
            return item.AllowedRoles.Contains(role);

        return true;
    }

    public static string GetRouteForRole(NavMenuItem item, UserRole role)
    {
        return item.Id switch
        {
            "leave-management" =>
                role == UserRole.Employee
                    ? "/emp-leave-management"
                    : "/admin-leave-management",

            "attendance" =>
                role == UserRole.Employee
                    ? "/emp-attendance-management"
                    : "/admin-attendance-management",

            _ => item.Route
        };
    }

    public static IReadOnlyDictionary<string, List<NavMenuItem>>
        GetMenuItemsGroupedBySection(UserRole role)
    {
        return GetMenuItemsByRole(role)
            .GroupBy(x => x.Section)
            .OrderBy(g => GetSectionOrder(g.Key))
            .ToDictionary(g => g.Key, g => g.ToList());
    }

    private static int GetSectionOrder(string section)
    {
        return section switch
        {
            "Main" => 1,
            "Leave" => 2,
            "Administration" => 3,
            "Account" => 4,
            _ => 99
        };
    }
}

public class NavMenuItem
{
    public string Id { get; set; } = string.Empty;
    public string Label { get; set; } = string.Empty;
    public string Icon { get; set; } = string.Empty;
    public string Section { get; set; } = string.Empty;
    public string Route { get; set; } = string.Empty;
    public List<UserRole> AllowedRoles { get; set; } = new();
    public List<UserRole> ExcludedRoles { get; set; } = new();
    public bool ExactMatch { get; set; } = false;
    public Func<UserRole, bool>? VisibilityCondition { get; set; }
}
