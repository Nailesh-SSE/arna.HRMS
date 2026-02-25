using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Authorization;
using Microsoft.JSInterop;
using System.Security.Claims;

namespace arna.HRMS.ClientServices.Auth;

public abstract class AuthenticatedLayoutBase : ComponentBase, IDisposable
{
    [Inject] protected AuthenticationStateProvider AuthProvider { get; set; } = default!;
    [Inject] protected NavigationManager Navigation { get; set; } = default!;
    [Inject] protected ILogger<AuthenticatedLayoutBase> Logger { get; set; } = default!; // Optional

    protected ClaimsPrincipal User { get; private set; } = new(new ClaimsIdentity());

    private int _userId;
    private int _employeeId;
    private string _userName = string.Empty;
    private string _userFullName = string.Empty;
    private string _role = string.Empty;
    private bool _isAuthenticated;

    // Optional: Add property to check initialization state
    protected bool IsInitialized { get; private set; }

    protected int GetUserId() => _userId;
    protected int GetEmployeeId() => _employeeId;
    protected string GetUserName() => _userName;
    protected string GetUserFullName() => _userFullName;
    protected string GetUserRole() => _role;
    protected bool IsAuthentication() => _isAuthenticated;

    protected override async Task OnInitializedAsync()
    {
        try
        {
            AuthProvider.AuthenticationStateChanged += OnAuthStateChanged;
            await LoadUserAsync();
            IsInitialized = true;
        }
        catch (Exception ex)
        {
            Logger.LogError(ex, "Error initializing AuthenticatedLayoutBase");
        }
    }

    private async void OnAuthStateChanged(Task<AuthenticationState> task) // Changed to async void for event handler
    {
        try
        {
            await HandleAuthChangedAsync(task);
        }
        catch (Exception ex)
        {
            Logger.LogError(ex, "Error handling auth state change");
        }
    }

    private async Task HandleAuthChangedAsync(Task<AuthenticationState> task)
    {
        var authState = await task;
        SetUser(authState.User);

        await RedirectIfUnauthenticatedAsync();
        await InvokeAsync(StateHasChanged);
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

    private void SetUser(ClaimsPrincipal user)
    {
        User = user;

        int.TryParse(user.FindFirst(ClaimTypes.NameIdentifier)?.Value, out _userId);
        int.TryParse(user.FindFirst("EmployeeId")?.Value, out _employeeId);

        _userName = user.FindFirst(ClaimTypes.Name)?.Value ?? string.Empty;
        _userFullName = user.FindFirst("FullName")?.Value ?? string.Empty;
        _role = user.FindFirst(ClaimTypes.Role)?.Value ?? string.Empty;
        _isAuthenticated = user.Identity?.IsAuthenticated ?? false;

        // Optional: Log user state for debugging
        Logger.LogDebug("User set: IsAuthenticated={IsAuthenticated}, UserId={UserId}, Role={Role}",
            _isAuthenticated, _userId, _role);
    }

    private async Task RedirectIfUnauthenticatedAsync()
    {
        if (_isAuthenticated || IsOnLoginPage())
            return;

        try
        {
            Logger.LogWarning("Redirecting unauthenticated user to login page");
            await InvokeAsync(() =>
                Navigation.NavigateTo("/login", replace: true));
        }
        catch (ObjectDisposedException)
        {
            // Component disposed, ignore
        }
        catch (JSDisconnectedException)
        {
            // Circuit disconnected, ignore
        }
        catch (Exception ex)
        {
            Logger.LogError(ex, "Error redirecting to login");
        }
    }

    private bool IsOnLoginPage()
    {
        var relative = Navigation.ToBaseRelativePath(Navigation.Uri);
        return relative.StartsWith("login", StringComparison.OrdinalIgnoreCase);
    }

    protected bool IsSuperAdmin() => GetUserRole() == Models.Enums.UserRole.SuperAdmin.ToString();
    protected bool IsAdmin() => GetUserRole() == Models.Enums.UserRole.Admin.ToString();
    protected bool IsEmployee() => GetUserRole() == Models.Enums.UserRole.Employee.ToString();

    public void Dispose()
    {
        try
        {
            AuthProvider.AuthenticationStateChanged -= OnAuthStateChanged;
        }
        catch (Exception ex)
        {
            Logger?.LogError(ex, "Error disposing AuthenticatedLayoutBase");
        }
    }
}