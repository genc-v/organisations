using System.Net;
using System.Text.Json;
using cmsOrg.Application.Common;

namespace cmsOrg.API.Middlewares;

public class ErrorHandlingMiddleware(RequestDelegate next)
{
    public async Task Invoke(HttpContext context)
    {
        try
        {
            await next(context);
        }
        catch (AppException ex)
        {
            context.Response.ContentType = "application/json";
            context.Response.StatusCode = ex.Code switch
            {
                1 => (int)HttpStatusCode.NotFound,
                3 => (int)HttpStatusCode.Forbidden,
                6 => (int)HttpStatusCode.Conflict,
                _ => (int)HttpStatusCode.BadRequest
            };
            await context.Response.WriteAsync(JsonSerializer.Serialize(new { ex.Code, ex.Message }));
        }
        catch (Exception ex)
        {
            context.Response.ContentType = "application/json";
            context.Response.StatusCode = (int)HttpStatusCode.InternalServerError;
            await context.Response.WriteAsync(JsonSerializer.Serialize(new { Message = ex.Message }));
        }
    }
}
