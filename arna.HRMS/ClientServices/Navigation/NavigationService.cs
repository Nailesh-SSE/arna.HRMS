using arna.HRMS.Models.Enums;
using System.Collections.Generic;

namespace arna.HRMS.ClientServices.Navigation;
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

/// <summary>
/// Service for managing navigation menu configuration
/// </summary>
public class NavigationService
{
    public static List<NavMenuItem> GetMenuItems()
    {
        return new List<NavMenuItem>
        {
            new NavMenuItem
            {
                Id = "dashboard",
                Label = "Dashboard",
                Icon = "bi bi-grid-1x2-fill",
                Section = "Main",
                Route = "/",
                ExactMatch = true,
                AllowedRoles = new() { UserRole.SuperAdmin, UserRole.Admin, UserRole.HR, UserRole.Manager, UserRole.Employee }
            },

            new NavMenuItem
            {
                Id = "employees",
                Label = "Employees",
                Icon = "bi bi-people-fill",
                Section = "Main",
                Route = "/employee",
                AllowedRoles = new() { UserRole.SuperAdmin, UserRole.Admin, UserRole.HR, UserRole.Manager,UserRole.Employee }
            },

            new NavMenuItem
            {
                Id = "attendance",
                Label = "Attendance",
                Icon = "bi bi-calendar2-check-fill",
                Section = "Main",
                Route = "/attendance",
                AllowedRoles = new() { UserRole.SuperAdmin, UserRole.Admin, UserRole.HR, UserRole.Manager, UserRole.Employee }
            },

            new NavMenuItem
            {
                Id = "leave-management",
                Label = "Leave Management",
                Icon = "bi bi-calendar-x-fill",
                Section = "Leave",
                Route = "/leave-management",
                AllowedRoles = new() { UserRole.SuperAdmin, UserRole.Admin, UserRole.HR, UserRole.Employee },
            },

            new NavMenuItem
            {
                Id = "department",
                Label = "Department",
                Icon = "bi bi-building-fill",
                Section = "Administration",
                Route = "/department",
                AllowedRoles = new() { UserRole.SuperAdmin, UserRole.Admin, UserRole.HR }
            },

            new NavMenuItem
            {
                Id = "role",
                Label = "Role",
                Icon = "bi bi-shield-fill-check",
                Section = "Administration",
                Route = "/role",
                AllowedRoles = new() { UserRole.SuperAdmin, UserRole.Admin, UserRole.HR }
            },

            new NavMenuItem
            {
                Id = "users",
                Label = "Users",
                Icon = "bi bi-person-fill-gear",
                Section = "Administration",
                Route = "/user",
                AllowedRoles = new() { UserRole.SuperAdmin, UserRole.Admin, UserRole.HR }
            },

            new NavMenuItem
            {
                Id = "profile",
                Label = "My Profile",
                Icon = "bi bi-person-badge-fill",
                Section = "Account",
                Route = "/profile",
                AllowedRoles = new() { UserRole.SuperAdmin, UserRole.Admin, UserRole.HR, UserRole.Manager, UserRole.Employee }
            }
        };
    }

    public static List<NavMenuItem> GetMenuItemsByRole(UserRole userRole)
    {
        var allItems = GetMenuItems();
        
        return allItems
            .Where(item => IsItemAccessibleToRole(item, userRole))
            .ToList();
    }

    public static bool IsItemAccessibleToRole(NavMenuItem item, UserRole userRole)
    {
        // Check visibility condition first
        if (item.VisibilityCondition != null && !item.VisibilityCondition(userRole))
            return false;

        // If excluded roles are specified, deny if user is in that list
        if (item.ExcludedRoles.Count > 0 && item.ExcludedRoles.Contains(userRole))
            return false;

        // If allowed roles are specified, only allow if user is in that list
        if (item.AllowedRoles.Count > 0)
            return item.AllowedRoles.Contains(userRole);

        // If no restrictions, allow access
        return true;
    }

    public static string GetRouteForRole(NavMenuItem item, UserRole userRole)
    {
        var route = item.Route;
        if (item.Id == "leave-management")
        {
            return userRole == UserRole.Employee 
                ? "/emp-leave-management" 
                : "/admin-leave-management";
        }

        // Smart routing for attendance
        if (item.Id == "attendance")
        {
            return userRole == UserRole.Employee 
                ? "/emp-attendance-management"
                : "/admin-attendance-management";
        }

        return route;
    }

    public static Dictionary<string, List<NavMenuItem>> GetMenuItemsGroupedBySection(UserRole userRole)
    {
        var items = GetMenuItemsByRole(userRole);
        
        return items
            .GroupBy(item => item.Section)
            .OrderBy(g => GetSectionOrder(g.Key))
            .ToDictionary(
                g => g.Key,
                g => g.ToList()
            );
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
