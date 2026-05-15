using System.Net.Http.Headers;

namespace arna.HRMS.Services.Auth;

public sealed class AuthHeaderHandler : DelegatingHandler
{
    private readonly CustomAuthStateProvider _authProvider;
    private readonly ILogger<AuthHeaderHandler> _logger;

    public AuthHeaderHandler(CustomAuthStateProvider authProvider, ILogger<AuthHeaderHandler> logger)
    {
        _authProvider = authProvider;
        _logger = logger;
    }

    protected override async Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
    {
        if (request.Headers.Authorization is null)
        {
            try
            {
                if (!request.Headers.Contains("User-Agent"))
                    request.Headers.Add("User-Agent", "arna.HRMS/1.0");
                if (request.Headers.Authorization is null)
                {
                    try
                    {
                        //var token = await _authProvider.GetAccessTokenAsync();

                        request.Options.TryGetValue(
                            new HttpRequestOptionsKey<CustomAuthStateProvider>("AuthProvider"),
                            out var scopedProvider);

                        var provider = scopedProvider ?? _authProvider; // ← only change

                        var token = await provider.GetAccessTokenAsync(); // ← was _authProvider
                        if (!string.IsNullOrWhiteSpace(token))
                            request.Headers.Authorization =
                                new AuthenticationHeaderValue("Bearer", token);
                    }
                    catch (Exception ex)
                    {
                        _logger.LogError(ex, "Failed to add authorization header");
                    }
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to add authorization header");
            }
        }

        return await base.SendAsync(request, cancellationToken);
    }
}