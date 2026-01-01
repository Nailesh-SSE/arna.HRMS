using arna.HRMS.ClientServices.Auth;
using arna.HRMS.Models.Common;
using Microsoft.AspNetCore.Components;
using System.Net;

namespace arna.HRMS.ClientServices.Common;

public class HttpService
{
    private readonly HttpClient _http;
    private readonly NavigationManager _navigationManager;
    private readonly CustomAuthStateProvider _customAuthStateProvider;

    public HttpService(HttpClient http, NavigationManager navigationManager, CustomAuthStateProvider customAuthStateProvider)
    {
        _http = http;
        _navigationManager = navigationManager;
        _customAuthStateProvider = customAuthStateProvider;
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
            var response = await request();
            var statusCode = (int)response.StatusCode;

            if (response.StatusCode == HttpStatusCode.Unauthorized)
            {
                await HandleUnauthorizedAsync();
                return ApiResult<T>.Fail(
                    "Session expired. Please login again.",
                    statusCode);
            }

            if (response.IsSuccessStatusCode)
            {
                if (typeof(T) == typeof(bool))
                {
                    return ApiResult<T>.Success(
                        (T)(object)true,
                        statusCode);
                }

                var data = await response.Content.ReadFromJsonAsync<T>();
                return ApiResult<T>.Success(data!, statusCode);
            }

            var errorMessage = await ReadErrorAsync(response);
            return ApiResult<T>.Fail(errorMessage, statusCode);
        }
        catch (Exception ex)
        {
            return ApiResult<T>.Fail(ex.Message, 500);
        }
    }

    private async Task HandleUnauthorizedAsync()
    {
        await _customAuthStateProvider.LogoutAsync();
        _navigationManager.NavigateTo("/login", forceLoad: true);
    }

    private static async Task<string> ReadErrorAsync(HttpResponseMessage response)
    {
        if (response.Content == null)
            return "Unexpected error occurred";

        var content = await response.Content.ReadAsStringAsync();
        return string.IsNullOrWhiteSpace(content)
            ? response.ReasonPhrase ?? "Request failed"
            : content;
    }
}
