namespace AMMS.Core.Models
{
    public class DataResult
    {
        public bool Success { get; init; }

        public string? ErrorCode { get; init; }

        public string? Message { get; init; }

        public static DataResult Ok() => new() { Success = true };

        public static DataResult Fail(string errorCode, string message) =>
            new() { Success = false, ErrorCode = errorCode, Message = message };
    }

    public class DataResult<T> : DataResult
    {
        public T? Data { get; init; }

        public static DataResult<T> Ok(T data) =>
            new() { Success = true, Data = data };

        public new static DataResult<T> Fail(string errorCode, string message) =>
            new() { Success = false, ErrorCode = errorCode, Message = message };
    }


}
