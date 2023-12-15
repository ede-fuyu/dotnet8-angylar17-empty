using Microsoft.AspNetCore.Mvc.Filters;

namespace Dotnet8App.Api.Infrastructure
{
    public class ErrorHandlingAttribute(ILogger<ErrorHandlingAttribute> logger) : ExceptionFilterAttribute
    {
        public override void OnException(ExceptionContext context)
        {
            var message = context.Exception.Message;
            logger.LogError(context.Exception, "An error occurred: {message}", message);

            context.Result = new InternalServerErrorResult();
            context.ExceptionHandled = true;

            base.OnException(context);
        }
    }
}