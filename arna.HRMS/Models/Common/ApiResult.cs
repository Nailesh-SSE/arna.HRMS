namespace arna.HRMS.Models.Common;

public class ApiResult<T>
{
    public bool IsSuccess { get; set; }
    public T? Data { get; set; }
    public string? ErrorMessage { get; set; }
    public int StatusCode { get; set; }

    public static ApiResult<T> Success(T data, int statusCode)
        => new()
        {
            IsSuccess = true,
            Data = data,
            StatusCode = statusCode
        };

    public static ApiResult<T> Fail(string error, int statusCode)
        => new()
        {
            IsSuccess = false,
            ErrorMessage = error,
            StatusCode = statusCode
        };
}
