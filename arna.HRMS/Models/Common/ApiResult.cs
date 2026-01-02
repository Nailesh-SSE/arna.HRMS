namespace arna.HRMS.Models.Common;

public class ApiResult<T>
{
    public bool IsSuccess { get; set; }
    public T? Data { get; set; }
    public string? Message { get; set; }
    public int StatusCode { get; set; }

    public static ApiResult<T> Success(T data, int statusCode = 200)
        => new() { IsSuccess = true, Data = data, StatusCode = statusCode };

    public static ApiResult<T> Fail(string message, int statusCode)
        => new() { IsSuccess = false, Message = message, StatusCode = statusCode };
}