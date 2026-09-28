namespace SahibindenClone.Application.Services;

public enum ServiceError
{
    None,
    NotFound,
    Forbidden,
    InvalidOperation,
    Conflict,
    Unauthorized
}

public sealed record ServiceResult<T>(T? Value, string? Message = null, ServiceError Error = ServiceError.None)
{
    public bool Succeeded => Error == ServiceError.None;

    public static ServiceResult<T> Success(T value) => new(value);
    public static ServiceResult<T> Failure(ServiceError error, string message) => new(default, message, error);
}
