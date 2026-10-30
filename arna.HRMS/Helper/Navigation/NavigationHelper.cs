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
            Id = "attendance-group",
            Label = "Attendance",
            Icon = "bi bi-calendar2-check-fill",
            Section = "Main",
            AllowedRoles = new() { UserRole.SuperAdmin, UserRole.Admin, UserRole.HR, UserRole.Manager, UserRole.Employee },
            Children = new()
            {
                new()
                {
                    Id = "attendance",
                    Label = "Attendance",
                    Icon = "bi bi-calendar-week",
                    Route = "/attendance",
                    AllowedRoles = new() { UserRole.SuperAdmin, UserRole.Admin, UserRole.HR, UserRole.Manager, UserRole.Employee }
                },
                new()
                {
                    Id = "attendanceRequest",
                    Label = "Attendance Requests",
                    Icon = "bi bi-inbox-fill",
                    Route = "/attendance-requests",
                    AllowedRoles = new() { UserRole.SuperAdmin, UserRole.Admin, UserRole.HR, UserRole.Manager, UserRole.Employee }
                }
            }
        },
        new()
        {
            Id = "leave-group",
            Label = "Leave",
            Icon = "bi bi-calendar-x-fill",
            Section = "Main",
            AllowedRoles = new() { UserRole.SuperAdmin, UserRole.Admin, UserRole.HR, UserRole.Manager, UserRole.Employee },
            Children = new()
            {
                new()
                {
                    Id = "leaveTypes",
                    Label = "Leave Types",
                    Icon = "bi bi-bookmark-check",
                    Route = "/leave-types",
                    AllowedRoles = new() { UserRole.SuperAdmin, UserRole.Admin, UserRole.HR }
                },
                new()
                {
                    Id = "leaveRequest",
                    Label = "Leave Requests",
                    Icon = "bi bi-inbox-fill",
                    Route = "/leave-requests",
                    AllowedRoles = new() { UserRole.SuperAdmin, UserRole.Admin, UserRole.HR, UserRole.Manager, UserRole.Employee }
                },
                new()
                {
                    Id = "festivalHoliday",
                    Label = "Festival Holidays",
                    Icon = "bi bi-calendar-heart",
                    Route = "/festival-holidays",
                    AllowedRoles = new() { UserRole.SuperAdmin, UserRole.Admin, UserRole.HR, UserRole.Manager, UserRole.Employee }
                }
            }
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
            Id = "attendanceSummaryReport",
            Label = "Attendance Report",
            Icon = "bi bi-clipboard-data me-2",
            Section = "Report",
            Route = "/admin-report-management",
            AllowedRoles = new() { UserRole.SuperAdmin, UserRole.Admin, UserRole.HR, UserRole.Manager, UserRole.Employee }
        },
         new()
        {
            Id = "leaveSummaryReport",
            Label = "Leave Report",
            Icon = "bi bi-clipboard-data me-2",
            Section = "Report",
            Route = "/leave-report",
            AllowedRoles = new() { UserRole.SuperAdmin, UserRole.Admin, UserRole.HR, UserRole.Manager}
        },
         new()
        {
            Id = "employeeleaveReport",
            Label = "Leave Report",
            Icon = "bi bi-clipboard-data me-2",
            Section = "Report",
            Route = "/employee-leave-report",
            AllowedRoles = new() { UserRole.Employee }
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
            .Select(item =>
            {
                if (item.Children == null || item.Children.Count == 0)
                    return item;

                var filteredChildren = item.Children
                    .Where(c => IsAccessible(c, role))
                    .ToList();

                return new NavMenuItem
                {
                    Id = item.Id,
                    Label = item.Label,
                    Icon = item.Icon,
                    Section = item.Section,
                    Route = item.Route,
                    AllowedRoles = item.AllowedRoles,
                    ExcludedRoles = item.ExcludedRoles,
                    ExactMatch = item.ExactMatch,
                    VisibilityCondition = item.VisibilityCondition,
                    Children = filteredChildren
                };
            })
            .Where(item => item.Children == null || item.Children.Count > 0 || !string.IsNullOrEmpty(item.Route))
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
            "leave-group" or "leave-management" =>
                role == UserRole.Employee
                    ? "/emp-leave-management"
                    : "/admin-leave-management",

            "leaveTypes" =>
                role == UserRole.Employee
                    ? "/emp-leave-management"
                    : "/admin-leave-types",

            "leaveRequest" =>
                role == UserRole.Employee
                    ? "/emp-leave-management"
                    : "/admin-leave-requests",

            "festivalHoliday" =>
                role == UserRole.Employee
                    ? "/emp-festival-holidays"
                    : "/admin-festival-holidays",

            "attendance" or "attendance-group" =>
                role == UserRole.Employee
                    ? "/emp-attendance-management"
                    : "/admin-attendance-management",

            "attendanceRequest" =>
                role == UserRole.Employee
                    ? "/emp-attendance-requests"
                    : "/admin-attendance-requests",

            "attendanceSummaryReport" => 
                role == UserRole.Employee 
                    ? "/employee-report"
                    : "/admin-report-management",
            
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
            "Report" =>4,
            "Account" => 5,
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
    public List<NavMenuItem> Children { get; set; } = new();
}
