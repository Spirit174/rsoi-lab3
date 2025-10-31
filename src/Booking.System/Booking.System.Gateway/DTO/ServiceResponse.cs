namespace Booking.System.Gateway.DTO;

public class ServiceResponse<T>
{
    public T? Response { get; }
    public int StatusCode { get; }
    public ErrorDto? Error { get; }
    public bool IsSuccess => StatusCode >= 200 && StatusCode < 300;
    public bool IsFallback { get; }

    public ServiceResponse(T? response, int statusCode = 200, ErrorDto? error = null, bool isFallback = false)
    {
        Response = response;
        StatusCode = statusCode;
        Error = error;
        IsFallback = isFallback;
    }

    public static ServiceResponse<T> Success(T response) => new(response, 200);
    public static ServiceResponse<T> Fallback(T fallback) => new(fallback, 200, null, true);
    public static ServiceResponse<T> ErrorResponse(string message, int statusCode = 500) 
        => new(default, statusCode, new ErrorDto(message));
    public static ServiceResponse<T> ServiceUnavailable(string serviceName) 
        => new(default, 503, new ErrorDto($"{serviceName} Service unavailable"), true);
    public string GetErrorMessage() => Error?.Message ?? "Unknown error";
}

public class ErrorDto
{
    public string Message { get; set; }

    public ErrorDto(string message)
    {
        Message = message;
    }
}