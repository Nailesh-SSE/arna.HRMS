// ============================================================
// FILE: arna.HRMS/Services/Http/HttpService.cs
// ============================================================
using arna.HRMS.Core.Common.Results;
using arna.HRMS.Models.Common.Result;
using arna.HRMS.Models.ViewModels.Auth;
using arna.HRMS.Services.Auth;
using System.Net;
using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;

namespace arna.HRMS.Services.Http;

// ──────────────────────────────────────────────────────────────
// HOW THIS WORKS:
//
//  Central HTTP client wrapper used by all ApiClients.
//  Every API call flows through here:
//
//  1. Attaches Bearer token to request (reads from CustomAuthStateProvider)
//  2. Sends request to API
//  3. If API returns 401 Unauthorized:
//       a. Tries to silently refresh the access token
//       b. If refresh succeeds → retries the original request
//       c. If refresh fails    → logs user out
//  4. Parses the API response (unwraps ServiceResult<T> wrapper)
//
//  TOKEN REFRESH RACE CONDITION PROTECTION:
//  SemaphoreSlim(_refreshLock) ensures that if 5 API calls all
//  get 401 simultaneously, only ONE refresh request is sent.
//  The other 4 wait for the lock, then use the new token.
// ──────────────────────────────────────────────────────────────

public sealed class HttpService
{
    private readonly HttpClient _httpClient;
    private readonly CustomAuthStateProvider _authProvider;
    private readonly ILogger<HttpService> _logger;

    private ApiClients? _apiClients;

    // One semaphore for the entire app — prevents concurrent refresh calls
    private static readonly SemaphoreSlim _refreshLock = new(1, 1);

    private static readonly JsonSerializerOptions JsonOptions =
        new() { PropertyNameCaseInsensitive = true };

    public HttpService(
        HttpClient httpClient,
        CustomAuthStateProvider authProvider,
        ILogger<HttpService> logger)
    {
        _httpClient = httpClient;
        _authProvider = authProvider;
        _logger = logger;
    }

    // Called by ApiClients constructor to avoid circular DI
    public void SetApiClients(ApiClients apiClients)
    {
        _apiClients = apiClients;
    }

    // ──────────────────────────────────────────────────────────
    // PUBLIC METHODS — used by ApiClients
    // ──────────────────────────────────────────────────────────

    public Task<ApiResult<T>> GetAsync<T>(string url, CancellationToken ct = default)
        => SendAsync<T>(HttpMethod.Get, url, null, ct);

    public Task<ApiResult<T>> PostAsync<T>(string url, object body, CancellationToken ct = default)
        => SendAsync<T>(HttpMethod.Post, url, body, ct);

    public Task<ApiResult<T>> PutAsync<T>(string url, object body, CancellationToken ct = default)
        => SendAsync<T>(HttpMethod.Put, url, body, ct);

    public Task<ApiResult<T>> DeleteAsync<T>(string url, CancellationToken ct = default)
        => SendAsync<T>(HttpMethod.Delete, url, null, ct);

    // ──────────────────────────────────────────────────────────
    // CORE PIPELINE
    // ──────────────────────────────────────────────────────────

    private async Task<ApiResult<T>> SendAsync<T>(
        HttpMethod method, string url, object? body, CancellationToken ct)
    {
        try
        {
            var response = await SendWithRefreshAsync(method, url, body, ct);
            return await ParseResponseAsync<T>(response);
        }
        catch (OperationCanceledException)
        {
            return ApiResult<T>.Fail("Request cancelled.", 499);
        }
        catch (HttpRequestException ex)
        {
            _logger.LogError(ex, "Network error calling {Url}", url);
            return ApiResult<T>.Fail("Network error occurred.", 503);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Unexpected error calling {Url}", url);
            return ApiResult<T>.Fail("Unexpected server error.", 500);
        }
    }

    // ──────────────────────────────────────────────────────────
    // SEND WITH AUTO TOKEN REFRESH ON 401
    // ──────────────────────────────────────────────────────────

    private async Task<HttpResponseMessage> SendWithRefreshAsync(
        HttpMethod method, string url, object? body, CancellationToken ct)
    {
        // First attempt
        var response = await SendRequestAsync(method, url, body, ct);

        // Not 401 — return as is
        if (response.StatusCode != HttpStatusCode.Unauthorized)
            return response;

        response.Dispose();

        // 401 received — try to refresh the access token silently
        var refreshed = await TryRefreshTokenAsync();

        if (!refreshed)
        {
            // Refresh failed — force logout
            await _authProvider.LogoutAsync();
            return new HttpResponseMessage(HttpStatusCode.Unauthorized);
        }

        // Retry original request with new token
        return await SendRequestAsync(method, url, body, ct);
    }

    // ──────────────────────────────────────────────────────────
    // BUILD AND SEND REQUEST
    // Reads JWT from CustomAuthStateProvider and adds Bearer header
    // ──────────────────────────────────────────────────────────

    private async Task<HttpResponseMessage> SendRequestAsync(
        HttpMethod method, string url, object? body, CancellationToken ct)
    {
        var request = new HttpRequestMessage(method, url);

        if (body != null)
        {
            var json = JsonSerializer.Serialize(body, JsonOptions);
            request.Content = new StringContent(json, Encoding.UTF8, "application/json");
        }

        // Read decrypted JWT from ProtectedSessionStorage
        var token = await _authProvider.GetAccessTokenAsync();

        if (!string.IsNullOrWhiteSpace(token))
        {
            request.Headers.Authorization =
                new AuthenticationHeaderValue("Bearer", token);
        }

        return await _httpClient.SendAsync(request, ct);
    }

    // ──────────────────────────────────────────────────────────
    // SILENT TOKEN REFRESH
    // Uses a lock to prevent multiple concurrent refresh calls
    // ──────────────────────────────────────────────────────────

    private async Task<bool> TryRefreshTokenAsync()
    {
        if (_apiClients == null)
            return false;

        // Wait for the lock — only 1 refresh call at a time
        await _refreshLock.WaitAsync();

        try
        {
            var (userId, refreshToken) = await _authProvider.GetRefreshDataAsync();

            if (userId <= 0 || string.IsNullOrWhiteSpace(refreshToken))
                return false;

            // Call API refresh endpoint (no auth header needed — AllowAnonymous)
            var result = await _apiClients.Auth.RefreshTokenAsync(
                new RefreshToken
                {
                    UserId = userId,
                    Token = refreshToken
                });

            if (!result.IsSuccess || result.Data == null)
                return false;

            // Save new tokens and update auth state
            await _authProvider.UpdateTokensAsync(
                result.Data.AccessToken,
                result.Data.RefreshToken ?? refreshToken);

            return true;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Token refresh failed");
            return false;
        }
        finally
        {
            _refreshLock.Release();
        }
    }

    // ──────────────────────────────────────────────────────────
    // PARSE API RESPONSE
    // Unwraps the ServiceResult<T> wrapper from API response
    // ──────────────────────────────────────────────────────────

    private async Task<ApiResult<T>> ParseResponseAsync<T>(HttpResponseMessage response)
    {
        using (response)
        {
            var statusCode = (int)response.StatusCode;

            // 204 No Content — success with no body
            if (response.StatusCode == HttpStatusCode.NoContent)
                return ApiResult<T>.Success(default!, statusCode);

            var raw = await response.Content.ReadAsStringAsync();

            // Non-success status — return error
            if (!response.IsSuccessStatusCode)
                return ApiResult<T>.Fail(raw, statusCode);

            if (string.IsNullOrWhiteSpace(raw))
                return ApiResult<T>.Success(default!, statusCode);

            try
            {
                // API wraps all responses in ServiceResult<T>
                // { isSuccess: true, message: "...", data: { ... } }
                var wrapper = JsonSerializer.Deserialize<ServiceResult<JsonElement>>(raw, JsonOptions);

                if (wrapper == null || !wrapper.IsSuccess)
                    return ApiResult<T>.Fail(wrapper?.Message ?? "Request failed.", statusCode);

                if (wrapper.Data.ValueKind is JsonValueKind.Null or JsonValueKind.Undefined)
                    return ApiResult<T>.Success(default!, statusCode);

                var data = JsonSerializer.Deserialize<T>(wrapper.Data.GetRawText(), JsonOptions);
                return ApiResult<T>.Success(data!, statusCode);
            }
            catch (JsonException ex)
            {
                _logger.LogError(ex, "JSON parsing failed. Raw: {Raw}", raw);
                return ApiResult<T>.Fail("Invalid JSON response.", statusCode);
            }
        }
    }
}