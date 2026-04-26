namespace cmsOrg.Application.Common;

public class AppException(int code, string message) : Exception(message)
{
    public int Code { get; } = code;

    public static AppException NotFound(string message = "The requested resource was not found.") => new(1, message);
    public static AppException Conflict(string message = "A conflict occurred with an existing resource.") => new(6, message);
    public static AppException Forbidden(string message = "You do not have access to this resource.") => new(3, message);
    public static AppException Unauthorized(string message = "Unauthorized access.") => new(16, message);
}
