using arna.HRMS.ClientServices.Auth;
using arna.HRMS.Core.Common.ServiceResult;
using arna.HRMS.Models.Common;
using arna.HRMS.Models.ViewModels;
using System.Net;
using System.Net.Http.Headers;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace arna.HRMS.ClientServices.Http;

public sealed class HttpService
{
    private readonly HttpClient _http;
    private readonly CustomAuthStateProvider _authProvider;
    private ApiClients? _apiClients;

    private static readonly SemaphoreSlim _refreshLock = new(1, 1);

    private static readonly JsonSerializerOptions JsonOptions = CreateJsonOptions();

    public HttpService(HttpClient http, CustomAuthStateProvider authProvider)
    {
        _http = http;
        _authProvider = authProvider;
    }

    public void SetApiClients(ApiClients apiClients) =>
        _apiClients = apiClients;

    // ===================== PUBLIC API =====================

    public Task<ApiResult<T>> GetAsync<T>(string url) =>
        SendAsync<T>(() => _http.GetAsync(url), url);

    public Task<ApiResult<T>> PostAsync<T>(string url, object data)
    {
        return SendAsync<T>(async () =>
        {
            var json = JsonSerializer.Serialize(data, JsonOptions);
            var content = new StringContent(json, System.Text.Encoding.UTF8, "application/json");

            // Force content length explicitly
            content.Headers.ContentLength = System.Text.Encoding.UTF8.GetByteCount(json);

            return await _http.PostAsync(url, content);
        }, url);
    }

    public Task<ApiResult<T>> PutAsync<T>(string url, object data) =>
        SendAsync<T>(() => _http.PutAsJsonAsync(url, data, JsonOptions), url);

    public Task<ApiResult<T>> DeleteAsync<T>(string url) =>
        SendAsync<T>(() => _http.DeleteAsync(url), url);

    // ===================== PIPELINE =====================

    private async Task<ApiResult<T>> SendAsync<T>(
        Func<Task<HttpResponseMessage>> request,
        string url)
    {
        try
        {
            await AttachTokenAsync();

            using var response = await request();

            if (response.StatusCode == HttpStatusCode.Unauthorized)
                return await HandleUnauthorizedAsync<T>(request, url);

            return await HandleResponseAsync<T>(response);
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

    // ===================== TOKEN HANDLING =====================

    private async Task AttachTokenAsync()
    {
        var token = await _authProvider.GetAccessTokenAsync();

        _http.DefaultRequestHeaders.Authorization =
            string.IsNullOrWhiteSpace(token)
            ? null
            : new AuthenticationHeaderValue("Bearer", token);
    }

    private async Task<ApiResult<T>> HandleUnauthorizedAsync<T>(
        Func<Task<HttpResponseMessage>> request,
        string url)
    {
        if (IsAuthEndpoint(url))
        {
            await _authProvider.LogoutAsync();
            return ApiResult<T>.Fail("Unauthorized.", 401);
        }

        var refreshed = await TrySilentRefreshAsync();

        if (!refreshed)
        {
            await _authProvider.LogoutAsync();
            return ApiResult<T>.Fail("Session expired. Please login again.", 401);
        }

        await AttachTokenAsync();

        using var retry = await request();
        return await HandleResponseAsync<T>(retry);
    }

    private static bool IsAuthEndpoint(string url) =>
        url.Contains("api/auth/login", StringComparison.OrdinalIgnoreCase) ||
        url.Contains("api/auth/refresh", StringComparison.OrdinalIgnoreCase);

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

            var refresh = await _apiClients.Auth.RefreshToken(new RefreshTokenViewModel
            {
                UserId = userId,
                RefreshToken = refreshToken
            });

            if (!refresh.IsSuccess || refresh.Data == null)
                return false;

            await _authProvider.UpdateTokensAsync(
                refresh.Data.AccessToken,
                refresh.Data.RefreshToken
            );

            return true;
        }
        finally
        {
            _refreshLock.Release();
        }
    }

    // ===================== RESPONSE HANDLING =====================

    private async Task<ApiResult<T>> HandleResponseAsync<T>(HttpResponseMessage response)
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

            if (wrapper == null)
                return ApiResult<T>.Fail("Invalid server response.", statusCode);

            if (!wrapper.IsSuccess)
                return ApiResult<T>.Fail(wrapper.Message ?? "Request failed.", statusCode);

            if (wrapper.Data.ValueKind == JsonValueKind.Null ||
                wrapper.Data.ValueKind == JsonValueKind.Undefined)
                return ApiResult<T>.Success(default!, statusCode);

            var data = JsonSerializer.Deserialize<T>(
                wrapper.Data.GetRawText(),
                JsonOptions);

            return ApiResult<T>.Success(data!, statusCode);
        }
        catch
        {
            return ApiResult<T>.Fail("Invalid JSON response.", statusCode);
        }
    }

    // ===================== JSON CONFIG =====================

    private static JsonSerializerOptions CreateJsonOptions()
    {
        var options = new JsonSerializerOptions
        {
            PropertyNameCaseInsensitive = true
        };

        options.Converters.Add(new TimeSpanJsonConverter());
        options.Converters.Add(new NullableTimeSpanJsonConverter());
        options.Converters.Add(new DateOnlyJsonConverter());
        options.Converters.Add(new NullableDateOnlyJsonConverter());

        return options;
    }

    // ===================== CONVERTERS =====================

    private sealed class TimeSpanJsonConverter : JsonConverter<TimeSpan>
    {
        public override TimeSpan Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
        {
            if (reader.TokenType == JsonTokenType.String &&
                TimeSpan.TryParse(reader.GetString(), out var ts))
                return ts;

            throw new JsonException("Invalid TimeSpan.");
        }

        public override void Write(Utf8JsonWriter writer, TimeSpan value, JsonSerializerOptions options) =>
            writer.WriteStringValue(value.ToString());
    }

    private sealed class NullableTimeSpanJsonConverter : JsonConverter<TimeSpan?>
    {
        public override TimeSpan? Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
        {
            if (reader.TokenType == JsonTokenType.Null)
                return null;

            if (reader.TokenType == JsonTokenType.String &&
                TimeSpan.TryParse(reader.GetString(), out var ts))
                return ts;

            throw new JsonException("Invalid TimeSpan.");
        }

        public override void Write(Utf8JsonWriter writer, TimeSpan? value, JsonSerializerOptions options)
        {
            if (value.HasValue)
                writer.WriteStringValue(value.Value.ToString());
            else
                writer.WriteNullValue();
        }
    }

    private sealed class DateOnlyJsonConverter : JsonConverter<DateOnly>
    {
        public override DateOnly Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
        {
            if (reader.TokenType == JsonTokenType.String &&
                DateOnly.TryParse(reader.GetString(), out var date))
                return date;

            throw new JsonException("Invalid DateOnly.");
        }

        public override void Write(Utf8JsonWriter writer, DateOnly value, JsonSerializerOptions options) =>
            writer.WriteStringValue(value.ToString("yyyy-MM-dd"));
    }

    private sealed class NullableDateOnlyJsonConverter : JsonConverter<DateOnly?>
    {
        public override DateOnly? Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
        {
            if (reader.TokenType == JsonTokenType.Null)
                return null;

            if (reader.TokenType == JsonTokenType.String &&
                DateOnly.TryParse(reader.GetString(), out var date))
                return date;

            throw new JsonException("Invalid DateOnly.");
        }

        public override void Write(Utf8JsonWriter writer, DateOnly? value, JsonSerializerOptions options)
        {
            if (value.HasValue)
                writer.WriteStringValue(value.Value.ToString("yyyy-MM-dd"));
            else
                writer.WriteNullValue();
        }
    }
}