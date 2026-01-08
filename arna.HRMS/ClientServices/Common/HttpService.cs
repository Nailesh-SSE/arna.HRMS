using arna.HRMS.ClientServices.Auth;
using arna.HRMS.Models.Common;
using System.Net;

namespace arna.HRMS.ClientServices.Common;

public class HttpService
{
    private readonly HttpClient _http;
    private readonly CustomAuthStateProvider _authProvider;

    public HttpService(HttpClient http, CustomAuthStateProvider authProvider)
    {
        _http = http;
        _authProvider = authProvider;
    }

    public Task<ApiResult<T>> GetAsync<T>(string url)
        => SendAsync<T>(() => _http.GetAsync(url));

    public Task<ApiResult<T>> PostAsync<T>(string url, object body)
        => SendAsync<T>(() => _http.PostAsJsonAsync(url, body));

    public Task<ApiResult<T>> PutAsync<T>(string url, object body)
        => SendAsync<T>(() => _http.PutAsJsonAsync(url, body));

    public Task<ApiResult<T>> DeleteAsync<T>(string url)
        => SendAsync<T>(() => _http.DeleteAsync(url));

    private async Task<ApiResult<T>> SendAsync<T>(
        Func<Task<HttpResponseMessage>> request)
    {
        try
        {
            using var response = await request();
            var statusCode = (int)response.StatusCode;

            if (response.StatusCode == HttpStatusCode.Unauthorized)
            {
                await HandleUnauthorizedAsync();
                return ApiResult<T>.Fail("Session expired. Please login again.", statusCode);
            }

            if (response.StatusCode == HttpStatusCode.Forbidden)
            {
                return ApiResult<T>.Fail("You do not have permission to perform this action.", statusCode);
            }

            if (response.StatusCode == HttpStatusCode.NoContent)
            {
                return ApiResult<T>.Success(default!, statusCode);
            }

            if (response.IsSuccessStatusCode)
            {
                if (typeof(T) == typeof(bool))
                {
                    return ApiResult<T>.Success((T)(object)true, statusCode);
                }

                var data = await SafeReadAsync<T>(response);
                return ApiResult<T>.Success(data!, statusCode);
            }

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

    private async Task HandleUnauthorizedAsync()
    {
        await _authProvider.LogoutAsync();
    }

    private static async Task<T?> SafeReadAsync<T>(HttpResponseMessage response)
    {
        try
        {
            return await response.Content.ReadFromJsonAsync<T>();
        }
        catch
        {
            return default;
        }
    }

    private static async Task<string?> ReadErrorAsync(HttpResponseMessage response)
    {
        if (response.Content == null)
            return null;

        var content = await response.Content.ReadAsStringAsync();
        return string.IsNullOrWhiteSpace(content)
            ? response.ReasonPhrase
            : content;
    }
}
