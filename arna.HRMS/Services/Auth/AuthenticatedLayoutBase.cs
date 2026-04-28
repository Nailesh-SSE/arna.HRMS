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
            AuthProvider.AuthenticationStateChanged += HandleAuthStateChanged;
            await LoadUserAsync();
            IsInitialized = true;
        }
        catch (Exception ex)
        {
            Logger.LogError(ex, "Error during layout initialization");
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

        int.TryParse(user.FindFirst(ClaimTypes.NameIdentifier)?.Value, out _userId);
        int.TryParse(user.FindFirst("EmployeeId")?.Value, out _employeeId);

        // ✅ FIX: Moved all assignments INSIDE a clear scope — the original code had a
        // misplaced brace that made it look like assignments were inside the if-block
        // but they were actually always executed. Made explicit now for clarity.
        if (user.Claims.Count() <= 0 || _employeeId <= 0)
        {
            Logger.LogWarning("Auth Fail — no claims or missing EmployeeId.");
        }

        _userName = user.FindFirst(ClaimTypes.Name)?.Value ?? string.Empty;
        _userFullName = user.FindFirst("FullName")?.Value ?? string.Empty;
        _role = user.FindFirst(ClaimTypes.Role)?.Value ?? string.Empty;
        _isAuthenticated = user.Identity?.IsAuthenticated ?? false;

        Logger.LogDebug(
            "User updated: Authenticated={Auth}, UserId={UserId}, Role={Role}",
            _isAuthenticated, _userId, _role);
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
            Logger.LogWarning("Unauthenticated access. Redirecting to login.");

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
        => GetUserRole() == Models.Enums.UserRole.SuperAdmin.ToString();

    protected bool IsAdmin()
        => GetUserRole() == Models.Enums.UserRole.Admin.ToString();

    protected bool IsEmployee()
        => GetUserRole() == Models.Enums.UserRole.Employee.ToString();

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