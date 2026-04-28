namespace arna.HRMS.Models.Common.Result;

public class ApiResult<T>
{
    public bool IsSuccess { get; private set; }
    public T? Data { get; private set; }
    public string? Message { get; private set; }
    public int StatusCode { get; private set; }

    private ApiResult() { }

    public static ApiResult<T> Success(T data, int statusCode = 200)
    {
        return new ApiResult<T>
        {
            IsSuccess = true,
            Data = data,
            StatusCode = statusCode
        };
    }

    public static ApiResult<T> Fail(string message, int statusCode = 400)
    {
        return new ApiResult<T>
        {
            IsSuccess = false,
            Message = message,
            StatusCode = statusCode
        };
    }
}