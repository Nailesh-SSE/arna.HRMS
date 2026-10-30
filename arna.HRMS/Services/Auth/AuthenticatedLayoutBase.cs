using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Authorization;
using Microsoft.JSInterop;
using System.Security.Claims;

namespace arna.HRMS.Services.Auth;

public abstract class AuthenticatedLayoutBase : ComponentBase, IDisposable
{
    [Inject] protected CustomAuthStateProvider AuthProvider { get; set; } = default!;
    [Inject] protected NavigationManager Navigation { get; set; } = default!;
    [Inject] protected ILogger<AuthenticatedLayoutBase> Logger { get; set; } = default!;

    protected ClaimsPrincipal User { get; private set; } =
        new(new ClaimsIdentity());

    private int _userId;
    private int _employeeId;
    private string _userName = string.Empty;
    private string _userFullName = string.Empty;
    private string _role = string.Empty;
    private bool _isAuthenticated;

    private bool _disposed;
    protected bool IsInitialized { get; private set; }

    // ==============================
    // Public Accessors
    // ==============================

    protected int GetUserId() => _userId;
    protected int GetEmployeeId() => _employeeId;
    protected string GetUserName() => _userName;
    protected string GetUserFullName() => _userFullName;
    protected string GetUserRole() => _role;
    protected bool IsAuthenticated() => _isAuthenticated;

    // ==============================
    // Lifecycle
    // ==============================

    protected override async Task OnInitializedAsync()
    {
        try
        {
            if (!IsInitialized)
            {
                AuthProvider.AuthenticationStateChanged += HandleAuthStateChanged;
                await LoadUserAsync();
                IsInitialized = true;
            }
        }
        catch (Exception ex)
        {
            Logger.LogError(ex, "Error during layout initialization");
        }
    }

    protected override async Task OnParametersSetAsync()
    {
        await base.OnParametersSetAsync();
        try
        {
            if (!IsInitialized)
            {
                AuthProvider.AuthenticationStateChanged += HandleAuthStateChanged;
                await LoadUserAsync();
                IsInitialized = true;
            }
        }
        catch (Exception ex)
        {
            Logger.LogError(ex, "Error during OnParametersSetAsync in AuthenticatedLayoutBase");
        }
    }


    // MUST be async void (event handler pattern)
    private async void HandleAuthStateChanged(Task<AuthenticationState> task)
    {
        // ✅ FIX: Guard against disposed component receiving auth state change events.
        // On production Blazor Server, circuits disconnect more aggressively under load.
        // The event handler can fire AFTER Dispose() was called (handler is mid-flight
        // when component is torn down). This check prevents ObjectDisposedException.
        if (_disposed) return;

        try
        {
            var authState = await task;
            SetUser(authState.User);
            await PopulateEmployeeIdFromStorageAsync();

            await RedirectIfUnauthenticatedAsync();

            if (!_disposed)
                await InvokeAsync(StateHasChanged);
        }
        catch (ObjectDisposedException)
        {
            // Component disposed, ignore
        }
        catch (JSDisconnectedException)
        {
            // Blazor Server circuit disconnected — this is the "disposed" error you see in logs
        }
        catch (Exception ex)
        {
            Logger.LogError(ex, "Error handling auth state change");
        }
    }

    protected async Task LoadUserAsync()
    {
        try
        {
            var authState = await AuthProvider.GetAuthenticationStateAsync();
            SetUser(authState.User);
            await PopulateEmployeeIdFromStorageAsync();
            await RedirectIfUnauthenticatedAsync();
        }
        catch (Exception ex)
        {
            Logger.LogError(ex, "Error loading user");
        }
    }

    // ==============================
    // User Handling
    // ==============================

    private void SetUser(ClaimsPrincipal user)
    {
        User = user;

        int.TryParse(
            user.FindFirst(ClaimTypes.NameIdentifier)?.Value
            ?? user.FindFirst("nameid")?.Value
            ?? user.FindFirst("UserId")?.Value
            ?? user.FindFirst("userId")?.Value
            ?? user.FindFirst("id")?.Value,
            out _userId);

        int.TryParse(
            user.FindFirst("EmployeeId")?.Value 
            ?? user.FindFirst("employeeId")?.Value,
            out _employeeId);

        _userName = user.FindFirst(ClaimTypes.Name)?.Value 
            ?? user.FindFirst("unique_name")?.Value
            ?? user.FindFirst("name")?.Value
            ?? user.FindFirst("Username")?.Value
            ?? string.Empty;

        _userFullName = user.FindFirst("FullName")?.Value 
            ?? user.FindFirst("fullName")?.Value 
            ?? user.FindFirst("name")?.Value 
            ?? string.Empty;

        _role = user.FindFirst(ClaimTypes.Role)?.Value 
            ?? user.FindFirst("role")?.Value 
            ?? user.FindFirst("Role")?.Value 
            ?? user.FindFirst("http://schemas.microsoft.com/ws/2008/06/identity/claims/role")?.Value 
            ?? string.Empty;

        _isAuthenticated = user.Identity?.IsAuthenticated ?? false;

        if (!_isAuthenticated)
        {
            Logger.LogDebug("No authenticated user is available for this layout instance.");
        }
        else if (_employeeId <= 0)
        {
            Logger.LogDebug("Authenticated user has no EmployeeId claim. UserId={UserId}, Role={Role}", _userId, _role);
        }

        Logger.LogDebug(
            "User updated: Authenticated={Auth}, UserId={UserId}, Role={Role}",
            _isAuthenticated, _userId, _role);
    }

    private async Task PopulateEmployeeIdFromStorageAsync()
    {
        if (!_isAuthenticated || _employeeId > 0)
            return;

        var storedEmployeeId = await AuthProvider.GetEmployeeIdAsync();
        if (storedEmployeeId > 0)
        {
            _employeeId = storedEmployeeId;
            Logger.LogDebug("EmployeeId loaded from protected storage. UserId={UserId}, EmployeeId={EmployeeId}", _userId, _employeeId);
        }
    }

    // ==============================
    // Redirect Handling
    // ==============================

    private async Task RedirectIfUnauthenticatedAsync()
    {
        if (_isAuthenticated || IsOnLoginPage())
            return;

        try
        {
            Logger.LogDebug("Unauthenticated access. Redirecting to login.");

            await InvokeAsync(() =>
                Navigation.NavigateTo("/login", replace: true));
        }
        catch (ObjectDisposedException)
        {
            // Safe ignore
        }
        catch (JSDisconnectedException)
        {
            // Safe ignore
        }
        catch (Exception ex)
        {
            Logger.LogError(ex, "Redirect failed");
        }
    }

    private bool IsOnLoginPage()
    {
        var relative = Navigation.ToBaseRelativePath(Navigation.Uri);
        return relative.StartsWith("login", StringComparison.OrdinalIgnoreCase);
    }

    // ==============================
    // Role Helpers
    // ==============================

    protected bool IsSuperAdmin()
    {
        var role = GetUserRole();
        return string.Equals(role, "SuperAdmin", StringComparison.OrdinalIgnoreCase)
            || string.Equals(role, "Super Admin", StringComparison.OrdinalIgnoreCase)
            || string.Equals(role, Models.Enums.UserRole.SuperAdmin.ToString(), StringComparison.OrdinalIgnoreCase);
    }

    protected bool IsAdmin()
    {
        var role = GetUserRole();
        return string.Equals(role, "Admin", StringComparison.OrdinalIgnoreCase)
            || string.Equals(role, "Administrator", StringComparison.OrdinalIgnoreCase)
            || string.Equals(role, Models.Enums.UserRole.Admin.ToString(), StringComparison.OrdinalIgnoreCase)
            || IsSuperAdmin();
    }

    protected bool IsEmployee()
    {
        var role = GetUserRole();
        return string.Equals(role, "Employee", StringComparison.OrdinalIgnoreCase)
            || string.Equals(role, Models.Enums.UserRole.Employee.ToString(), StringComparison.OrdinalIgnoreCase);
    }


    // ==============================
    // Dispose
    // ==============================

    public void Dispose()
    {
        _disposed = true;

        try
        {
            AuthProvider.AuthenticationStateChanged -= HandleAuthStateChanged;
        }
        catch (Exception ex)
        {
            Logger.LogError(ex, "Error during layout disposal");
        }
    }
}
