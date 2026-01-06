using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Authorization;
using System.Security.Claims;
using arna.HRMS.Models.Enums;
using Microsoft.JSInterop;

namespace arna.HRMS.Components.Auth;

public abstract class AuthenticatedLayoutBase : ComponentBase, IDisposable
{
    [Inject] protected AuthenticationStateProvider AuthProvider { get; set; } = default!;
    [Inject] protected NavigationManager Navigation { get; set; } = default!;

    protected ClaimsPrincipal User { get; private set; } = new(new ClaimsIdentity());

    protected int GetUserId() 
        => int.TryParse(User.FindFirst(ClaimTypes.NameIdentifier)?.Value, out var id)? id: 0;

    protected int GetEmployeeId()
        => int.TryParse(User.FindFirst("EmployeeId")?.Value,out var id)? id: 0;

    protected string GetUserRole()
        => User.FindFirst(ClaimTypes.Role)?.Value ?? string.Empty;

    protected ClaimsPrincipal GetUser() => User;

    protected bool IsAuthenticated => User.Identity?.IsAuthenticated ?? false;

    protected override void OnInitialized()
    {
        AuthProvider.AuthenticationStateChanged += OnAuthChanged;
    }

    protected override async Task OnParametersSetAsync()
    {
        var authState = await AuthProvider.GetAuthenticationStateAsync();
        SetUser(authState.User);

        await RedirectIfUnauthenticatedAsync();
    }

    private void OnAuthChanged(Task<AuthenticationState> task)
    {
        _ = HandleAuthChangedAsync(task);
    }

    private async Task HandleAuthChangedAsync(Task<AuthenticationState> task)
    {
        var authState = await task;
        SetUser(authState.User);

        await RedirectIfUnauthenticatedAsync();
        await InvokeAsync(StateHasChanged);
    }

    private async Task RedirectIfUnauthenticatedAsync()
    {
        if (IsAuthenticated || IsOnLoginPage())
            return;

        try
        {
            await InvokeAsync(() => Navigation.NavigateTo("/login", replace: true));
        }
        catch (ObjectDisposedException) { }
        catch (JSDisconnectedException) { }
    }

    private bool IsOnLoginPage()
    {
        var relative = Navigation.ToBaseRelativePath(Navigation.Uri);
        return relative.StartsWith("login", StringComparison.OrdinalIgnoreCase);
    }

    private void SetUser(ClaimsPrincipal user)
    {
        User = user;
    }

    protected bool IsSuperAdmin() => User.IsInRole(UserRole.SuperAdmin.ToString());

    protected bool IsAdmin() => User.IsInRole(UserRole.Admin.ToString());

    public void Dispose()
    {
        AuthProvider.AuthenticationStateChanged -= OnAuthChanged;
    }
}
