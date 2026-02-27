using arna.HRMS.ClientServices.Auth;
using arna.HRMS.Core.Common.ServiceResult;
using arna.HRMS.Models.Common;
using arna.HRMS.Models.ViewModels;
using System.Net;
using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace arna.HRMS.ClientServices.Http;

public sealed class HttpService
{
    private readonly HttpClient _http;
    private readonly CustomAuthStateProvider _authProvider;
    private readonly ILogger<HttpService> _logger;
    private ApiClients? _apiClients;

    private static readonly SemaphoreSlim _refreshLock = new(1, 1);
    private static readonly JsonSerializerOptions JsonOptions = CreateJsonOptions();

    public HttpService(HttpClient http,
                       CustomAuthStateProvider authProvider,
                       ILogger<HttpService> logger)
    {
        _http = http;
        _authProvider = authProvider;
        _logger = logger;
    }

    public void SetApiClients(ApiClients apiClients)
        => _apiClients = apiClients;

    // =========================================================
    // PUBLIC METHODS
    // =========================================================

    public Task<ApiResult<T>> GetAsync<T>(string url)
        => SendAsync<T>(() =>
            new HttpRequestMessage(HttpMethod.Get, url), url);

    public Task<ApiResult<T>> DeleteAsync<T>(string url)
        => SendAsync<T>(() =>
            new HttpRequestMessage(HttpMethod.Delete, url), url);

    public Task<ApiResult<T>> PostAsync<T>(string url, object body)
        => SendAsync<T>(() =>
        {
            var json = JsonSerializer.Serialize(body, JsonOptions);
            var request = new HttpRequestMessage(HttpMethod.Post, url);
            request.Content = new StringContent(json, Encoding.UTF8, "application/json");
            return request;
        }, url);

    public Task<ApiResult<T>> PutAsync<T>(string url, object body)
        => SendAsync<T>(() =>
        {
            var json = JsonSerializer.Serialize(body, JsonOptions);
            var request = new HttpRequestMessage(HttpMethod.Put, url);
            request.Content = new StringContent(json, Encoding.UTF8, "application/json");
            return request;
        }, url);

    // =========================================================
    // CORE PIPELINE
    // =========================================================

    private async Task<ApiResult<T>> SendAsync<T>(
        Func<HttpRequestMessage> requestFactory,
        string url)
    {
        try
        {
            _logger.LogDebug("Sending request to {Url}", url);

            var request = requestFactory();

            // Ensure token is set on request
            var token = await _authProvider.GetAccessTokenAsync();
            if (!string.IsNullOrWhiteSpace(token))
            {
                request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", token);
                _logger.LogDebug("Bearer token added to request");
            }

            var response = await _http.SendAsync(request);

            if (response.StatusCode == HttpStatusCode.Unauthorized)
            {
                _logger.LogWarning("Received 401 Unauthorized, attempting token refresh");

                var refreshed = await TrySilentRefreshAsync();

                if (refreshed)
                {
                    _logger.LogInformation("Token refresh successful, retrying request");
                    // Retry with new token
                    var retryRequest = requestFactory();
                    var newToken = await _authProvider.GetAccessTokenAsync();
                    if (!string.IsNullOrWhiteSpace(newToken))
                    {
                        retryRequest.Headers.Authorization = new AuthenticationHeaderValue("Bearer", newToken);
                    }
                    response.Dispose();
                    response = await _http.SendAsync(retryRequest);
                }
                else
                {
                    _logger.LogWarning("Token refresh failed, logging out user");
                    response.Dispose();
                    await _authProvider.LogoutAsync();
                    return ApiResult<T>.Fail("Session expired. Please login again.", 401);
                }
            }

            return await HandleResponseAsync<T>(response);
        }
        catch (HttpRequestException ex)
        {
            _logger.LogError(ex, "Network error occurred");
            return ApiResult<T>.Fail($"Network error: {ex.Message}", 503);
        }
        catch (TaskCanceledException ex)
        {
            _logger.LogError(ex, "Request timeout");
            return ApiResult<T>.Fail("Request timed out.", 408);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Unexpected error occurred");
            return ApiResult<T>.Fail(ex.Message, 500);
        }
    }

    private async Task<bool> TrySilentRefreshAsync()
    {
        if (_apiClients == null)
        {
            _logger.LogError("ApiClients not initialized");
            return false;
        }

        await _refreshLock.WaitAsync();

        try
        {
            var (userId, refreshToken) = await _authProvider.GetRefreshDataAsync();

            if (userId <= 0 || string.IsNullOrWhiteSpace(refreshToken))
            {
                _logger.LogWarning("Invalid refresh data: UserId={UserId}, HasRefreshToken={HasToken}", userId, !string.IsNullOrWhiteSpace(refreshToken));
                return false;
            }

            _logger.LogDebug("Attempting silent refresh for user {UserId}", userId);

            var refresh = await _apiClients.Auth.RefreshToken(new RefreshTokenViewModel
            {
                UserId = userId,
                RefreshToken = refreshToken
            });

            if (!refresh.IsSuccess || refresh.Data == null)
            {
                _logger.LogWarning("Refresh failed: IsSuccess={IsSuccess}, HasData={HasData}", refresh.IsSuccess, refresh.Data != null);
                return false;
            }

            if (string.IsNullOrWhiteSpace(refresh.Data.AccessToken))
            {
                _logger.LogWarning("Refresh response missing access token");
                return false;
            }

            await _authProvider.UpdateTokensAsync(
                refresh.Data.AccessToken,
                refresh.Data.RefreshToken ?? refreshToken
            );

            _logger.LogInformation("Token refresh successful");
            return true;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error during token refresh");
            return false;
        }
        finally
        {
            _refreshLock.Release();
        }
    }

    // =========================================================
    // RESPONSE HANDLING
    // =========================================================

    private async Task<ApiResult<T>> HandleResponseAsync<T>(HttpResponseMessage response)
    {
        using (response)
        {
            var statusCode = (int)response.StatusCode;

            if (response.StatusCode == HttpStatusCode.NoContent)
            {
                _logger.LogDebug("Received 204 No Content");
                return ApiResult<T>.Success(default!, statusCode);
            }

            var raw = await response.Content.ReadAsStringAsync();

            if (!response.IsSuccessStatusCode)
            {
                _logger.LogWarning("Request failed with status {StatusCode}: {Response}", statusCode, raw);
                return ApiResult<T>.Fail(raw, statusCode);
            }

            if (string.IsNullOrWhiteSpace(raw))
            {
                _logger.LogDebug("Response is empty");
                return ApiResult<T>.Success(default!, statusCode);
            }

            try
            {
                var wrapper = JsonSerializer.Deserialize<ServiceResult<JsonElement>>(raw, JsonOptions);

                if (wrapper == null)
                {
                    _logger.LogError("Failed to deserialize response wrapper");
                    return ApiResult<T>.Fail("Invalid server response.", statusCode);
                }

                if (!wrapper.IsSuccess)
                {
                    _logger.LogWarning("Server returned unsuccessful result: {Message}", wrapper.Message);
                    return ApiResult<T>.Fail(wrapper.Message ?? "Request failed.", statusCode);
                }

                if (wrapper.Data.ValueKind == JsonValueKind.Null ||
                    wrapper.Data.ValueKind == JsonValueKind.Undefined)
                {
                    _logger.LogDebug("Response data is null");
                    return ApiResult<T>.Success(default!, statusCode);
                }

                var data = JsonSerializer.Deserialize<T>(
                    wrapper.Data.GetRawText(),
                    JsonOptions);

                _logger.LogDebug("Response successfully deserialized");
                return ApiResult<T>.Success(data!, statusCode);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error deserializing response");
                return ApiResult<T>.Fail("Invalid JSON response.", statusCode);
            }
        }
    }

    // =========================================================
    // JSON CONFIG
    // =========================================================

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

    // =========================================================
    // CONVERTERS
    // =========================================================

    private sealed class TimeSpanJsonConverter : JsonConverter<TimeSpan>
    {
        public override TimeSpan Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
        {
            if (reader.TokenType == JsonTokenType.String &&
                TimeSpan.TryParse(reader.GetString(), out var ts))
                return ts;

            throw new JsonException("Invalid TimeSpan.");
        }

        public override void Write(Utf8JsonWriter writer, TimeSpan value, JsonSerializerOptions options)
            => writer.WriteStringValue(value.ToString());
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

        public override void Write(Utf8JsonWriter writer, DateOnly value, JsonSerializerOptions options)
            => writer.WriteStringValue(value.ToString("yyyy-MM-dd"));
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