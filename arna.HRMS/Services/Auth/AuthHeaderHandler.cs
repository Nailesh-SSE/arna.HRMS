using Microsoft.AspNetCore.Http;
using System.Net.Http.Headers;

namespace arna.HRMS.Services.Auth;

public sealed class AuthHeaderHandler : DelegatingHandler
{
    private readonly IHttpContextAccessor _httpContextAccessor;
    private const string AccessTokenKey = "auth_access_token";

    public AuthHeaderHandler(IHttpContextAccessor httpContextAccessor)
    {
        _httpContextAccessor = httpContextAccessor;
    }

    protected override async Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
    {
        if (request.Headers.Authorization is null)
        {
            var httpContext = _httpContextAccessor.HttpContext;
            if (httpContext != null && httpContext.Request.Cookies.TryGetValue(AccessTokenKey, out var token))
            {
                if (!string.IsNullOrWhiteSpace(token))
                {
                    request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", token);
                }
            }
        }

        return await base.SendAsync(request, cancellationToken);
    }
}