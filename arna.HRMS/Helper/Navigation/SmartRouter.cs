using arna.HRMS.Models.Enums;

namespace arna.HRMS.Helper.Navigation;

public interface ISmartRouter
{
    string GetRoute(string featureName, UserRole userRole);
    void NavigateToFeature(string featureName, UserRole userRole, Action<string> navigationCallback);
}

public sealed class SmartRouter : ISmartRouter
{
    private static readonly IReadOnlyDictionary<string, IReadOnlyDictionary<UserRole, string>> FeatureRoutes
        = new Dictionary<string, IReadOnlyDictionary<UserRole, string>>(StringComparer.OrdinalIgnoreCase)
        {
            ["leave-management"] = new Dictionary<UserRole, string>
            {
                [UserRole.Employee] = "/emp-leave-management",
                [UserRole.Admin] = "/admin-leave-management",
                [UserRole.SuperAdmin] = "/admin-leave-management"
            },

            ["attendance"] = new Dictionary<UserRole, string>
            {
                [UserRole.Employee] = "/emp-attendance-management",
                [UserRole.Admin] = "/admin-attendance-management",
                [UserRole.SuperAdmin] = "/admin-attendance-management"
            },

            ["dashboard"] = new Dictionary<UserRole, string>
            {
                [UserRole.Employee] = "/",
                [UserRole.Admin] = "/",
                [UserRole.SuperAdmin] = "/"
            }
        };

    public string GetRoute(string featureName, UserRole userRole)
    {
        if (string.IsNullOrWhiteSpace(featureName))
            return "/";

        if (!FeatureRoutes.TryGetValue(featureName.Trim(), out var roleRoutes))
            return "/";

        if (roleRoutes.TryGetValue(userRole, out var route))
            return route;

        return roleRoutes.Values.FirstOrDefault() ?? "/";
    }

    public void NavigateToFeature(
        string featureName,
        UserRole userRole,
        Action<string> navigationCallback)
    {
        if (navigationCallback is null)
            return;

        var route = GetRoute(featureName, userRole);
        navigationCallback(route);
    }
}
