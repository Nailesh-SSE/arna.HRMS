using arna.HRMS.ClientServices.Auth;
using arna.HRMS.Core.Common.ServiceResult;
using arna.HRMS.Models.Common;
using arna.HRMS.Models.ViewModels;
using System.Net;
using System.Text.Json;

namespace arna.HRMS.ClientServices.Http;

public class HttpService
{
    private readonly HttpClient _http;
    private readonly CustomAuthStateProvider _authProvider;

    private ApiClients? _apiClients;
    private static readonly SemaphoreSlim _refreshLock = new(1, 1);

    private static readonly JsonSerializerOptions _jsonOptions = new()
    {
        PropertyNameCaseInsensitive = true
    };

    public HttpService(HttpClient http, CustomAuthStateProvider authProvider)
    {
        _http = http;
        _authProvider = authProvider;
    }

    public void SetApiClients(ApiClients apiClients)
    {
        _apiClients = apiClients;
    }

    public Task<ApiResult<T>> GetAsync<T>(string url)
        => SendAsync<T>(() => _http.GetAsync(url), url);

    public Task<ApiResult<T>> PostAsync<T>(string url, object body)
        => SendAsync<T>(() => _http.PostAsJsonAsync(url, body), url);

    public Task<ApiResult<T>> DeleteAsync<T>(string url)
        => SendAsync<T>(() => _http.DeleteAsync(url), url);

    private async Task<ApiResult<T>> SendAsync<T>(
        Func<Task<HttpResponseMessage>> request,
        string url)
    {
        try
        {
            using var response = await request();
            var statusCode = (int)response.StatusCode;

            if (response.StatusCode == HttpStatusCode.Unauthorized)
            {
                if (url.Contains("api/auth/login") || url.Contains("api/auth/refresh"))
                {
                    await _authProvider.LogoutAsync();
                    return ApiResult<T>.Fail("Unauthorized.", statusCode);
                }

                var refreshed = await TrySilentRefreshAsync();

                if (!refreshed)
                {
                    await _authProvider.LogoutAsync();
                    return ApiResult<T>.Fail("Session expired. Please login again.", statusCode);
                }

                using var retryResponse = await request();
                return await HandleResponse<T>(retryResponse);
            }

            return await HandleResponse<T>(response);
        }
        catch (HttpRequestException ex)
        {
            return ApiResult<T>.Fail($"Network error: {ex.Message}", 503);
        }
        catch (TaskCanceledException)
        {
            return ApiResult<T>.Fail("Request timed out.", 408);
        }
        catch (Exception ex)
        {
            return ApiResult<T>.Fail(ex.Message, 500);
        }
    }

    private async Task<ApiResult<T>> HandleResponse<T>(HttpResponseMessage response)
    {
        var statusCode = (int)response.StatusCode;

        if (response.StatusCode == HttpStatusCode.Forbidden)
            return ApiResult<T>.Fail("You do not have permission to perform this action.", statusCode);

        if (response.StatusCode == HttpStatusCode.NoContent)
            return ApiResult<T>.Success(default!, statusCode);

        if (!response.IsSuccessStatusCode)
        {
            var error = await ReadErrorAsync(response);

            return response.StatusCode switch
            {
                HttpStatusCode.BadRequest =>
                    ApiResult<T>.Fail(error ?? "Invalid request.", statusCode),

                HttpStatusCode.NotFound =>
                    ApiResult<T>.Fail("Resource not found.", statusCode),

                HttpStatusCode.Conflict =>
                    ApiResult<T>.Fail(error ?? "Conflict occurred.", statusCode),

                HttpStatusCode.InternalServerError =>
                    ApiResult<T>.Fail("Server error occurred.", statusCode),

                _ =>
                    ApiResult<T>.Fail(error ?? "Request failed.", statusCode)
            };
        }

        var raw = response.Content == null ? null : await response.Content.ReadAsStringAsync();

        if (string.IsNullOrWhiteSpace(raw))
        {
            if (typeof(T) == typeof(bool))
                return ApiResult<T>.Success((T)(object)true, statusCode);

            return ApiResult<T>.Success(default!, statusCode);
        }

        try
        {
            var serviceResult = JsonSerializer.Deserialize<ServiceResult<T>>(raw, _jsonOptions);

            if (serviceResult == null)
                return ApiResult<T>.Fail("Invalid response from server.", statusCode);

            if (!serviceResult.IsSuccess)
                return ApiResult<T>.Fail(serviceResult.Message ?? "Request failed.", statusCode);

            if (typeof(T) == typeof(bool) && serviceResult.Data == null)
                return ApiResult<T>.Success((T)(object)true, statusCode);

            return ApiResult<T>.Success(serviceResult.Data!, statusCode);
        }
        catch
        {
            try
            {
                var data = JsonSerializer.Deserialize<T>(raw, _jsonOptions);
                return ApiResult<T>.Success(data!, statusCode);
            }
            catch
            {
                return ApiResult<T>.Fail($"Invalid JSON response: {raw}", statusCode);
            }
        }
    }

    private async Task<bool> TrySilentRefreshAsync()
    {
        if (_apiClients == null)
            return false;

        await _refreshLock.WaitAsync();
        try
        {
            var (userId, refreshToken) = await _authProvider.GetRefreshDataAsync();

            if (userId <= 0 || string.IsNullOrWhiteSpace(refreshToken))
                return false;

            var refreshResult = await _apiClients.Auth.RefreshToken(new RefreshTokenViewModel
            {
                UserId = userId,
                RefreshToken = refreshToken
            });

            if (!refreshResult.IsSuccess || refreshResult.Data == null)
                return false;

            if (string.IsNullOrWhiteSpace(refreshResult.Data.AccessToken))
                return false;

            await _authProvider.UpdateTokensAsync(
                refreshResult.Data.AccessToken,
                refreshResult.Data.RefreshToken
            );

            return true;
        }
        finally
        {
            _refreshLock.Release();
        }
    }

    private static async Task<string?> ReadErrorAsync(HttpResponseMessage response)
    {
        if (response.Content == null)
            return response.ReasonPhrase;

        var content = await response.Content.ReadAsStringAsync();

        return string.IsNullOrWhiteSpace(content)
            ? response.ReasonPhrase
            : content;
    }
}
