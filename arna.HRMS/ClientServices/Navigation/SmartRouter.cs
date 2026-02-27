using arna.HRMS.Models.Enums;

namespace arna.HRMS.ClientServices.Navigation;
public interface ISmartRouter
{
    string GetRoute(string featureName, UserRole userRole);
    void NavigateToFeature(string featureName, UserRole userRole, Action<string> navigationCallback);
}

public class SmartRouter : ISmartRouter
{
    private static readonly Dictionary<string, Dictionary<UserRole, string>> FeatureRoutes = new()
    {
        // Leave Management - Routes based on role
        ["leave-management"] = new Dictionary<UserRole, string>
        {
            { UserRole.Employee, "/emp-leave-management" },
            { UserRole.Admin, "/admin-leave-management" },
            { UserRole.SuperAdmin, "/admin-leave-management" }
        },

        // Attendance - Routes based on role
        ["attendance"] = new Dictionary<UserRole, string>
        {
            { UserRole.Employee, "/emp-attendance-management" },
            { UserRole.Admin, "/admin-attendance-management" },
            { UserRole.SuperAdmin, "/admin-attendance-management" }
        },

        // Dashboard - Same for all roles
        ["dashboard"] = new Dictionary<UserRole, string>
        {
            { UserRole.Employee, "/" },
            { UserRole.Admin, "/" },
            { UserRole.SuperAdmin, "/" }
        },

        // Add more features as needed
    };

    public string GetRoute(string featureName, UserRole userRole)
    {
        var feature = featureName.ToLower().Trim();

        if (FeatureRoutes.TryGetValue(feature, out var routes))
        {
            if (routes.TryGetValue(userRole, out var route))
            {
                return route;
            }

            // Fallback to first available route if role not explicitly defined
            return routes.Values.FirstOrDefault() ?? "/";
        }

        // Default fallback
        return "/";
    }

    public void NavigateToFeature(string featureName, UserRole userRole, Action<string> navigationCallback)
    {
        var route = GetRoute(featureName, userRole);
        navigationCallback(route);
    }
}
