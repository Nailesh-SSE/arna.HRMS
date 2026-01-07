using arna.HRMS.Models.Common;

namespace arna.HRMS.ClientServices.Common;

public class ApiEndpoint<T>
{
    private readonly HttpService _http;
    private readonly string _baseUrl;

    public ApiEndpoint(HttpService http, string baseUrl)
    {
        _http = http;
        _baseUrl = baseUrl;
    }

    public Task<ApiResult<List<T>>> GetAllAsync()
        => _http.GetAsync<List<T>>(_baseUrl);

    public Task<ApiResult<T>> GetByIdAsync(int id)
        => _http.GetAsync<T>($"{_baseUrl}/{id}");

    public Task<ApiResult<T>> CreateAsync(T dto)
        => _http.PostAsync<T>(_baseUrl, dto);

    public Task<ApiResult<bool>> UpdateAsync(int id, T dto)
        => _http.PutAsync<bool>($"{_baseUrl}/{id}", dto);

    public Task<ApiResult<bool>> DeleteAsync(int id)
        => _http.DeleteAsync<bool>($"{_baseUrl}/{id}");
}
