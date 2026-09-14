using HealthDataCollector.Enums;

namespace HealthDataCollector.Models
{
    public class ServiceResult<T>
    {
        public bool Success { get; init; }
        public string? Message { get; init; }
        public T? Data { get; init; }
        public ServiceError Error { get; init; }

        public static ServiceResult<T> Ok(T data, string? message = null)
            => new()
            {
                Success = true,
                Data = data,
                Message = message,
                Error = ServiceError.None
            };

        public static ServiceResult<T> Fail(ServiceError errorCode, string message)
            => new()
            {
                Success = false,
                Message = message,
                Error = errorCode
            };
    }
}
