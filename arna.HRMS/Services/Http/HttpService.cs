using arna.HRMS.Core.Common.Results;
using arna.HRMS.Models.Common.Result;
using arna.HRMS.Models.ViewModels.Auth;
using arna.HRMS.Services.Auth;
using System.Net;
using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;

namespace arna.HRMS.Services.Http;

public sealed class HttpService
{
    private readonly HttpClient _httpClient;
    private readonly CustomAuthStateProvider _authProvider;
    private readonly ILogger<HttpService> _logger;
    private ApiClients? _apiClients;

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

        var token = await _authProvider.GetAccessTokenAsync();

        if (!string.IsNullOrWhiteSpace(token))
        {
            request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", token);
        }

        return await _httpClient.SendAsync(request, ct);
    }

    // =====================================================
    // TOKEN REFRESH
    // =====================================================

    private async Task<bool> TryRefreshTokenAsync()
    {
        if (_apiClients == null)
            return false;

        await _refreshLock.WaitAsync();

        try
        {
            var (userId, refreshToken) = await _authProvider.GetRefreshDataAsync();

            if (userId <= 0 || string.IsNullOrWhiteSpace(refreshToken))
                return false;

            var result = await _apiClients.Auth.RefreshTokenAsync(
                new RefreshToken
                {
                    UserId = userId,
                    Token = refreshToken
                });

            if (!result.IsSuccess || result.Data == null)
                return false;

            await _authProvider.UpdateTokensAsync(result.Data.AccessToken, result.Data.RefreshToken ?? refreshToken);

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