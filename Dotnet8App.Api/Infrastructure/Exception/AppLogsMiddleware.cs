using Serilog.Context;

namespace Dotnet8App.Api.Infrastructure;

public class AppLogsMiddleware(RequestDelegate next)
{
    public Task Invoke(HttpContext context)
    {
        LogContext.PushProperty("UserName", context.User?.Identity?.Name ?? null);
        LogContext.PushProperty("RequestUri", context.Request.Path);
        LogContext.PushProperty("StatusCodes", context.Response.StatusCode);

        return next(context);
    }
}
