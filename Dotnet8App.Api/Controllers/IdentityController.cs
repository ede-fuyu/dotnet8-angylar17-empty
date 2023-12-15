using Dotnet8App.Api.Infrastructure;
using Microsoft.AspNetCore.Mvc;

namespace Dotnet8App.Api.Controllers
{
    [ApiController]
    [JwtAuthorize]
    [Route("[controller]/[action]")]
    public class IdentityController : ControllerBase
    {
        /// <summary>
        /// 取得 JWT Token 中的所有 Claims
        /// </summary>
        /// <returns></returns>
        [HttpGet]
        public IActionResult Claims()
        {
            var user = HttpContext.User;
            return Ok(user.Claims.Select(p => new { p.Type, p.Value }));
        }

        /// <summary>
        /// 取得 JWT Token 中的使用者名稱
        /// </summary>
        /// <returns></returns>
        [HttpGet]
        public IActionResult Username()
        {
            var user = HttpContext.User;
            return Ok(user.Identity?.Name);
        }

        /// <summary>
        /// 取得使用者是否擁有特定角色
        /// </summary>
        /// <returns></returns>
        [HttpGet]
        public IActionResult IsInRole(string name)
        {
            var user = HttpContext.User;
            return Ok(user.IsInRole(name));
        }

        /// <summary>
        /// 取得 JWT Token 中的 JWT ID
        /// </summary>
        /// <returns></returns>
        [HttpGet]
        public IActionResult JwtId()
        {
            var user = HttpContext.User;
            return Ok(user.Claims.FirstOrDefault(p => p.Type == "jti")?.Value);
        }

        /// <summary>
        /// 取得 JWT Token 中的 使用者資訊
        /// </summary>
        /// <returns></returns>
        [HttpGet]
        public IActionResult GetUserProfile()
        {
            var user = HttpContext.User;
            var userEMail = user.Claims.FirstOrDefault(p => p.Type == "UserMail")?.Value;
            return Ok(new {userName = user.Identity?.Name, userEMail });
        }
    }
}
