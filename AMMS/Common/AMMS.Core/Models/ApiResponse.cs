namespace AMMS.Core.Models
{
    public class ApiResponse
    {
        public bool Success { get; init; }

        public string? ErrorCode { get; init; }

        public string? Message { get; init; }

        public object? Details { get; init; }

        public DateTime Timestamp { get; init; } = DateTime.UtcNow;

        public string? TraceId { get; init; }

        public static ApiResponse Ok(string? traceId = null) => new() { Success = true, TraceId = traceId };

        public static ApiResponse Fail(string errorCode,string message,object? details = null, string? traceId = null) =>
            new()
            {
                Success = false,
                ErrorCode = errorCode,
                Message = message,
                Details = details,
                TraceId = traceId
            };
    }

    public class ApiResponse<T> : ApiResponse
    {
        public T? Data { get; init; }

        public static ApiResponse<T> Ok(T data, string? traceId = null) =>
            new() { Success = true, Data = data, TraceId = traceId };

        public new static ApiResponse<T> Fail(string errorCode,string message,object? details = null,string? traceId = null) =>
            new()
            {
                Success = false,
                ErrorCode = errorCode,
                Message = message,
                Details = details,
                TraceId = traceId
            };
    }
}
