// ============================================================
// FILE: arna.HRMS/Services/Auth/AuthenticatedLayoutBase.cs
// ============================================================
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Authorization;
using Microsoft.JSInterop;
using System.Security.Claims;

namespace arna.HRMS.Services.Auth;

// ──────────────────────────────────────────────────────────────
// HOW THIS WORKS:
//
//  Base class for pages/layouts that need the current user info.
//  Inherit from this instead of ComponentBase to get:
//    - GetUserId(), GetUserRole(), GetEmployeeId() etc.
//    - Automatic redirect to /login if not authenticated
//    - Automatic re-render on auth state change (login/logout)
//
//  USAGE:
//    @inherits AuthenticatedLayoutBase
//    ... then use GetUserId(), GetUserRole() etc. in code
// ──────────────────────────────────────────────────────────────

public abstract class AuthenticatedLayoutBase : ComponentBase, IDisposable
{
    [Inject] protected AuthenticationStateProvider AuthProvider { get; set; } = default!;
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

    private volatile bool _disposed;

    protected bool IsInitialized { get; private set; }

    // ── Public Accessors ──────────────────────────────────────
    protected int GetUserId() => _userId;
    protected int GetEmployeeId() => _employeeId;
    protected string GetUserName() => _userName;
    protected string GetUserFullName() => _userFullName;
    protected string GetUserRole() => _role;
    protected bool IsAuthenticated() => _isAuthenticated;

    // ── Lifecycle ─────────────────────────────────────────────
    protected override async Task OnInitializedAsync()
    {
        try
        {
            // Subscribe first so we don't miss any state change
            AuthProvider.AuthenticationStateChanged += HandleAuthStateChanged;

            // Load current user — cached by CustomAuthStateProvider, so no extra storage hit
            await LoadUserAsync();

            IsInitialized = true;
        }
        catch (Exception ex)
        {
            Logger.LogError(ex, "Error during layout initialization");
        }
    }

    // Event handler — async void is required for event handler pattern
    private async void HandleAuthStateChanged(Task<AuthenticationState> task)
    {
        if (_disposed) return;

        try
        {
            var authState = await task;
            SetUser(authState.User);
            await RedirectIfUnauthenticatedAsync();

            if (!_disposed)
                await InvokeAsync(StateHasChanged);
        }
        catch (ObjectDisposedException) { /* component disposed, safe to ignore */ }
        catch (JSDisconnectedException) { /* SignalR circuit disconnected */        }
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

    // ── User Parsing from Claims ───────────────────────────────
    private void SetUser(ClaimsPrincipal user)
    {
        User = user;

        int.TryParse(user.FindFirst(ClaimTypes.NameIdentifier)?.Value, out _userId);
        int.TryParse(user.FindFirst("EmployeeId")?.Value, out _employeeId);

        _userName = user.FindFirst(ClaimTypes.Name)?.Value ?? string.Empty;
        _userFullName = user.FindFirst("FullName")?.Value ?? string.Empty;
        _role = user.FindFirst(ClaimTypes.Role)?.Value ?? string.Empty;
        _isAuthenticated = user.Identity?.IsAuthenticated ?? false;

        Logger.LogDebug(
            "Auth state updated: Authenticated={Auth}, UserId={UserId}, Role={Role}",
            _isAuthenticated, _userId, _role);
    }

    // ── Redirect ───────────────────────────────────────────────
    private async Task RedirectIfUnauthenticatedAsync()
    {
        if (_isAuthenticated || IsOnLoginPage())
            return;

        try
        {
            Logger.LogWarning("Unauthenticated — redirecting to /login");
            await InvokeAsync(() => Navigation.NavigateTo("/login", replace: true));
        }
        catch (ObjectDisposedException) { /* safe ignore */ }
        catch (JSDisconnectedException) { /* safe ignore */ }
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

    // ── Role Helpers ───────────────────────────────────────────
    protected bool IsSuperAdmin() => GetUserRole() == Models.Enums.UserRole.SuperAdmin.ToString();
    protected bool IsAdmin() => GetUserRole() == Models.Enums.UserRole.Admin.ToString();
    protected bool IsEmployee() => GetUserRole() == Models.Enums.UserRole.Employee.ToString();

    // ── Dispose ────────────────────────────────────────────────
    public void Dispose()
    {
        _disposed = true;

        try
        {
            AuthProvider.AuthenticationStateChanged -= HandleAuthStateChanged;
        }
        catch (Exception ex)
        {
            Logger.LogError(ex, "Error during dispose");
        }
    }
}