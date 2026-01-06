using arna.HRMS.Models.Enums;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Authorization;
using Microsoft.JSInterop;
using System.Security.Claims;

namespace arna.HRMS.Components.Auth;

public abstract class AuthenticatedLayoutBase : ComponentBase, IDisposable
{
    [Inject] protected AuthenticationStateProvider AuthProvider { get; set; } = default!;
    [Inject] protected NavigationManager Navigation { get; set; } = default!;

    protected ClaimsPrincipal User { get; private set; } = new(new ClaimsIdentity());

    private int UserId;
    private int EmployeeId;
    private string UserName = string.Empty;
    private string UserFullName = string.Empty;
    private string Role = string.Empty;
    private bool IsAuthenticated;

    protected int GetUserId() => UserId;
    protected int GetEmployeeId() => EmployeeId;
    protected string GetUserName() => UserName;
    protected string GetUserFullName() => UserFullName;
    protected string GetUserRole() => Role;
    protected bool IsAuthentication() => IsAuthenticated;

    protected override async Task OnInitializedAsync()
    {
        AuthProvider.AuthenticationStateChanged += OnAuthStateChanged;
        await LoadUserAsync();
    }

    private void OnAuthStateChanged(Task<AuthenticationState> task)
        => _ = HandleAuthChangedAsync(task);

    private async Task HandleAuthChangedAsync(Task<AuthenticationState> task)
    {
        var authState = await task;
        SetUser(authState.User);

        await RedirectIfUnauthenticatedAsync();
        await InvokeAsync(StateHasChanged);
    }

    protected async Task LoadUserAsync()
    {
        var authState = await AuthProvider.GetAuthenticationStateAsync();
        SetUser(authState.User);
        await RedirectIfUnauthenticatedAsync();
    }

    private void SetUser(ClaimsPrincipal user)
    {
        User = user;

        int.TryParse(user.FindFirst(ClaimTypes.NameIdentifier)?.Value, out UserId);
        int.TryParse(user.FindFirst("EmployeeId")?.Value, out EmployeeId);

        UserName = user.FindFirst(ClaimTypes.Name)?.Value ?? string.Empty;
        UserFullName = user.FindFirst("FullName")?.Value ?? string.Empty;
        Role = user.FindFirst(ClaimTypes.Role)?.Value ?? string.Empty;
        IsAuthenticated = user.Identity?.IsAuthenticated ?? false;
    }

    private async Task RedirectIfUnauthenticatedAsync()
    {
        if (IsAuthentication() || IsOnLoginPage())
            return;

        try
        {
            await InvokeAsync(() =>
                Navigation.NavigateTo("/login", replace: true));
        }
        catch (ObjectDisposedException) { }
        catch (JSDisconnectedException) { }
    }

    private bool IsOnLoginPage()
    {
        var relative = Navigation.ToBaseRelativePath(Navigation.Uri);
        return relative.StartsWith("login", StringComparison.OrdinalIgnoreCase);
    }

    protected bool IsSuperAdmin() => GetUserRole() == UserRole.SuperAdmin.ToString();

    protected bool IsAdmin() => GetUserRole() == UserRole.Admin.ToString();

    public void Dispose()
    {
        AuthProvider.AuthenticationStateChanged -= OnAuthStateChanged;
    }
}
