using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Authorization;

public abstract class AuthenticatedLayoutBase : LayoutComponentBase, IDisposable
{
    [Inject] protected AuthenticationStateProvider AuthStateProvider { get; set; } = default!;
    [Inject] protected NavigationManager Navigation { get; set; } = default!;

    protected override void OnInitialized()
    {
        AuthStateProvider.AuthenticationStateChanged += OnAuthenticationStateChanged;
    }

    private async void OnAuthenticationStateChanged(Task<AuthenticationState> task)
    {
        var authState = await task;

        if (authState.User.Identity?.IsAuthenticated != true)
        {
            Navigation.NavigateTo("/login", replace: true);
        }
    }

    public void Dispose()
    {
        AuthStateProvider.AuthenticationStateChanged -= OnAuthenticationStateChanged;
    }
}
