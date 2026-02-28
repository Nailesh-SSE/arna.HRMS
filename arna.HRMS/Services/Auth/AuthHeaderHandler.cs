using System.Net.Http.Headers;

namespace arna.HRMS.Services.Auth;

public sealed class AuthHeaderHandler : DelegatingHandler
{
    private readonly CustomAuthStateProvider _authProvider;

    public AuthHeaderHandler(CustomAuthStateProvider authProvider)
    {
        _authProvider = authProvider;
    }

    protected override async Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
    {
        if (request.Headers.Authorization is null)
        {
            var token = await _authProvider.GetAccessTokenAsync();

            if (!string.IsNullOrWhiteSpace(token))
            {
                request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", token);
            }
        }

        return await base.SendAsync(request, cancellationToken);
    }
}
