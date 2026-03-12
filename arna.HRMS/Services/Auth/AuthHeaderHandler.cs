// ============================================================
// FILE: arna.HRMS/Services/Auth/AuthHeaderHandler.cs
// ============================================================
using System.Net.Http.Headers;

namespace arna.HRMS.Services.Auth;

// ──────────────────────────────────────────────────────────────
// HOW THIS WORKS:
//
//  This is a DelegatingHandler — it sits in the HttpClient pipeline.
//  Before every HTTP request goes out to the API, this handler
//  automatically attaches the JWT Bearer token to the
//  Authorization header.
//
//  It reads the token from CustomAuthStateProvider (which reads
//  from ProtectedSessionStorage — decrypted, real JWT).
//
//  WHY NOT IHttpContextAccessor?
//  In Blazor Server, HttpContext is only valid during the initial
//  HTTP handshake. After that the connection is WebSocket/SignalR.
//  IHttpContextAccessor.HttpContext is NULL during Blazor interactions.
//  Also, ProtectedSessionStorage encrypts cookies — the raw cookie
//  value is NOT a JWT, it's an encrypted blob.
// ──────────────────────────────────────────────────────────────

public sealed class AuthHeaderHandler : DelegatingHandler
{
    private readonly CustomAuthStateProvider _authProvider;

    public AuthHeaderHandler(CustomAuthStateProvider authProvider)
    {
        _authProvider = authProvider;
    }

    protected override async Task<HttpResponseMessage> SendAsync(
        HttpRequestMessage request,
        CancellationToken cancellationToken)
    {
        // Only set if not already set — HttpService also sets it directly
        // this handler acts as a safety fallback
        if (request.Headers.Authorization is null)
        {
            var token = await _authProvider.GetAccessTokenAsync();

            if (!string.IsNullOrWhiteSpace(token))
            {
                request.Headers.Authorization =
                    new AuthenticationHeaderValue("Bearer", token);
            }
        }

        return await base.SendAsync(request, cancellationToken);
    }
}