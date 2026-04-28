using arna.HRMS.Core.Common.Results;
using arna.HRMS.Models.Common.Result;
using arna.HRMS.Models.ViewModels.Auth;
using arna.HRMS.Services.Auth;
using System.Net;
using System.Text;
using System.Text.Json;

namespace arna.HRMS.Services.Http;

public sealed class HttpService
{
    private readonly HttpClient _httpClient;
    private readonly IHttpClientFactory _httpClientFactory;
    private readonly CustomAuthStateProvider _authProvider;
    private readonly ILogger<HttpService> _logger;
    private ApiClients? _apiClients;

    // ✅ FIX: Removed 'static' — was shared across ALL users on Blazor Server.
    // Static SemaphoreSlim means if User A triggers a refresh, User B is BLOCKED.
    // If the semaphore was never released (exception), ALL users would deadlock.
    // Each scoped HttpService instance now has its own lock.
    private readonly SemaphoreSlim _refreshLock = new(1, 1);

    private static readonly JsonSerializerOptions JsonOptions =
        new() { PropertyNameCaseInsensitive = true };

    public HttpService(
        HttpClient httpClient,
        IHttpClientFactory httpClientFactory,
        CustomAuthStateProvider authProvider,
        ILogger<HttpService> logger)
    {
        _httpClient = httpClient;
        _httpClientFactory = httpClientFactory;
        _authProvider = authProvider;
        _logger = logger;
    }

    public void SetApiClients(ApiClients apiClients)
    {
        _apiClients = apiClients;
    }

    // =====================================================
    // PUBLIC METHODS
    // =====================================================

    public async Task<ApiResult<T>> GetAsync<T>(string url, CancellationToken ct = default)
    {
        return await SendAsync<T>(HttpMethod.Get, url, null, ct);
    }

    public async Task<ApiResult<T>> PostAsync<T>(string url, object body, CancellationToken ct = default)
    {
        return await SendAsync<T>(HttpMethod.Post, url, body, ct);
    }

    public async Task<ApiResult<T>> DeleteAsync<T>(string url, CancellationToken ct = default)
    {
        return await SendAsync<T>(HttpMethod.Delete, url, null, ct);
    }

    public async Task<ApiResult<T>> PutAsync<T>(string url, object body, CancellationToken ct = default)
    {
        return await SendAsync<T>(HttpMethod.Put, url, body, ct);
    }

    // =====================================================
    // CORE PIPELINE
    // =====================================================

    private async Task<ApiResult<T>> SendAsync<T>(HttpMethod method, string url, object? body, CancellationToken ct)
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
            _logger.LogError(ex, "Network error");
            return ApiResult<T>.Fail("Network error occurred.", 503);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Unexpected error");
            return ApiResult<T>.Fail("Unexpected server error.", 500);
        }
    }

    // =====================================================
    // RETRY + REFRESH
    // =====================================================

    private async Task<HttpResponseMessage> SendWithRefreshAsync(HttpMethod method, string url, object? body, CancellationToken ct)
    {
        var response = await SendRequestAsync(method, url, body, ct);

        if (response.StatusCode != HttpStatusCode.Unauthorized)
            return response;

        response.Dispose();

        var refreshed = await TryRefreshTokenAsync();

        if (!refreshed)
        {
            await _authProvider.LogoutAsync();
            return new HttpResponseMessage(HttpStatusCode.Unauthorized);
        }

        return await SendRequestAsync(method, url, body, ct);
    }

    private async Task<HttpResponseMessage> SendRequestAsync(HttpMethod method, string url, object? body, CancellationToken ct)
    {
        var request = new HttpRequestMessage(method, url);

        if (body != null)
        {
            var json = JsonSerializer.Serialize(body, JsonOptions);
            request.Content = new StringContent(json, Encoding.UTF8, "application/json");
        }

        // ✅ FIX: Removed duplicate manual token injection here.
        // AuthHeaderHandler (registered as DelegatingHandler on "AuthorizedClient") already
        // injects the Bearer token at the pipeline level. Having both caused double-header
        // attempts and race conditions during token refresh scenarios.
        // The AuthHeaderHandler checks: if (request.Headers.Authorization is null) → sets it.
        // So we just let the pipeline handle it cleanly.

        return await _httpClient.SendAsync(request, ct);
    }

    // =====================================================
    // TOKEN REFRESH
    // =====================================================

    private async Task<bool> TryRefreshTokenAsync()
    {
        await _refreshLock.WaitAsync();

        try
        {
            var (userId, refreshToken) = await _authProvider.GetRefreshDataAsync();

            if (userId <= 0 || string.IsNullOrWhiteSpace(refreshToken))
                return false;

            // ✅ FIX: Use the dedicated "RefreshClient" (no AuthHeaderHandler attached)
            // instead of going through _apiClients which uses the same HttpService pipeline.
            // Using _apiClients.Auth.RefreshTokenAsync() here caused an INFINITE LOOP:
            //   SendAsync → 401 → TryRefreshTokenAsync → RefreshTokenAsync → SendAsync
            //   → 401 (refresh endpoint also returns 401 with expired token) → infinite loop
            // → eventually causes ObjectDisposedException / stack overflow on the server.
            var refreshHttpClient = _httpClientFactory.CreateClient("RefreshClient");

            var requestBody = JsonSerializer.Serialize(new
            {
                UserId = userId,
                // ✅ Also fixes: Frontend RefreshToken VM had "Token" property but backend expects "RefreshToken"
                RefreshToken = refreshToken
            }, JsonOptions);

            var response = await refreshHttpClient.PostAsync(
                "api/auth/refresh",
                new StringContent(requestBody, Encoding.UTF8, "application/json"));

            if (!response.IsSuccessStatusCode)
                return false;

            var raw = await response.Content.ReadAsStringAsync();

            if (string.IsNullOrWhiteSpace(raw))
                return false;

            var wrapper = JsonSerializer.Deserialize<ServiceResult<JsonElement>>(raw, JsonOptions);

            if (wrapper == null || !wrapper.IsSuccess)
                return false;

            if (wrapper.Data.ValueKind is JsonValueKind.Null or JsonValueKind.Undefined)
                return false;

            var data = JsonSerializer.Deserialize<AuthResponse>(wrapper.Data.GetRawText(), JsonOptions);

            if (data == null || string.IsNullOrWhiteSpace(data.AccessToken))
                return false;

            await _authProvider.UpdateTokensAsync(
                data.AccessToken,
                data.RefreshToken ?? refreshToken);

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

    // =====================================================
    // RESPONSE PARSING
    // =====================================================

    private async Task<ApiResult<T>> ParseResponseAsync<T>(HttpResponseMessage response)
    {
        using (response)
        {
            var statusCode = (int)response.StatusCode;

            if (response.StatusCode == HttpStatusCode.NoContent)
                return ApiResult<T>.Success(default!, statusCode);

            var raw = await response.Content.ReadAsStringAsync();

            if (!response.IsSuccessStatusCode)
                return ApiResult<T>.Fail(raw, statusCode);

            if (string.IsNullOrWhiteSpace(raw))
                return ApiResult<T>.Success(default!, statusCode);

            try
            {
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
                _logger.LogError(ex, "JSON parsing failed");
                return ApiResult<T>.Fail("Invalid JSON response.", statusCode);
            }
        }
    }
}