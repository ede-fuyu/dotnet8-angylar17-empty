using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Infrastructure;

namespace Dotnet8App.Api.Infrastructure
{
    [DefaultStatusCode(DefaultStatusCode)]
    public class InternalServerErrorResult : StatusCodeResult
    {
        private const int DefaultStatusCode = StatusCodes.Status500InternalServerError;

        public InternalServerErrorResult() : base(DefaultStatusCode)
        {
        }
    }
}