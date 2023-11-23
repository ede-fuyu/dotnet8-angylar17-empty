using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;

namespace Dotnet8App.Api.Infrastructure
{
    [AttributeUsage(AttributeTargets.Class | AttributeTargets.Method)]
    public class JwtAuthorizeAttribute(params string[] permissions) : Attribute, IAuthorizationFilter
    {
        public void OnAuthorization(AuthorizationFilterContext context)
        {
            // 使用 IoC 容器解析相應的服務
            var serviceProvider = context.HttpContext.RequestServices;
            var jwtManager = serviceProvider.GetRequiredService<JwtManager>();

            var havePermission = false;
            // 從 HTTP 請求的 Header 中獲取 Authorization 標頭
            var authorizationHeader = context.HttpContext.Request.Headers.Authorization;

            // 檢查 Authorization 標頭是否存在且以 "Bearer " 開頭
            if (!string.IsNullOrEmpty(authorizationHeader) && authorizationHeader.ToString().StartsWith("Bearer "))
            {
                // 提取 JWT Token
                var jwtToken = authorizationHeader.ToString()["Bearer ".Length..];

                // 進行 JWT 驗證
                havePermission = jwtManager.VerifyToken(jwtToken, permissions);
            }

            if (!havePermission)
            {
                context.Result = new UnauthorizedResult();
            }
        }
    }
}
