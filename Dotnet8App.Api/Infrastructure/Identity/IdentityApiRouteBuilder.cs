using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.Data;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.WebUtilities;
using System.ComponentModel.DataAnnotations;
using System.Diagnostics;
using System.Security.Claims;
using System.Text;
using System.Text.Encodings.Web;

namespace Dotnet8App.Api.Infrastructure;

public static class IdentityApiRouteBuilder
{
    private static readonly EmailAddressAttribute _emailAddressAttribute = new();

    public static IEndpointConventionBuilder MapIdentityApiEndpointRoute(this IEndpointRouteBuilder endpoints, bool IsDevelopment)
    {
        ArgumentNullException.ThrowIfNull(endpoints);

        var routeGroup = endpoints.MapGroup("/identity");

        // 開發模式 追加註冊使用者的api
        if (IsDevelopment)
        {
            string? confirmEmailEndpointName = null;

            var emailSender = endpoints.ServiceProvider.GetRequiredService<IEmailSender<IdentityUser>>();
            var linkGenerator = endpoints.ServiceProvider.GetRequiredService<LinkGenerator>();

            // NOTE: We cannot inject UserManager<TUser> directly because the TUser generic parameter is currently unsupported by RDG.
            // https://github.com/dotnet/aspnetcore/issues/47338
            routeGroup.MapPost("/register", async Task<Results<Ok, ValidationProblem>>
                ([FromBody] RegisterRequest registration, HttpContext context, [FromServices] IServiceProvider sp) =>
            {
                var userManager = sp.GetRequiredService<UserManager<IdentityUser>>();

                if (!userManager.SupportsUserEmail)
                {
                    throw new NotSupportedException($"{nameof(MapIdentityApiEndpointRoute)} requires a user store with email support.");
                }

                var userStore = sp.GetRequiredService<IUserStore<IdentityUser>>();
                var emailStore = (IUserEmailStore<IdentityUser>)userStore;
                var email = registration.Email;

                if (string.IsNullOrEmpty(email) || !_emailAddressAttribute.IsValid(email))
                {
                    return CreateValidationProblem(IdentityResult.Failed(userManager.ErrorDescriber.InvalidEmail(email)));
                }

                var user = new IdentityUser();
                await userStore.SetUserNameAsync(user, email, CancellationToken.None);
                await emailStore.SetEmailAsync(user, email, CancellationToken.None);
                var result = await userManager.CreateAsync(user, registration.Password);

                if (!result.Succeeded)
                {
                    return CreateValidationProblem(result);
                }

                await SendConfirmationEmailAsync(user, userManager, context, email);
                return TypedResults.Ok();
            });

            async Task SendConfirmationEmailAsync(IdentityUser user, UserManager<IdentityUser> userManager, HttpContext context, string email, bool isChange = false)
            {
                if (confirmEmailEndpointName is null)
                {
                    throw new NotSupportedException("No email confirmation endpoint was registered!");
                }

                var code = isChange
                    ? await userManager.GenerateChangeEmailTokenAsync(user, email)
                    : await userManager.GenerateEmailConfirmationTokenAsync(user);
                code = WebEncoders.Base64UrlEncode(Encoding.UTF8.GetBytes(code));

                var userId = await userManager.GetUserIdAsync(user);
                var routeValues = new RouteValueDictionary()
                {
                    ["userId"] = userId,
                    ["code"] = code,
                };

                if (isChange)
                {
                    // This is validated by the /confirmEmail endpoint on change.
                    routeValues.Add("changedEmail", email);
                }

                var confirmEmailUrl = linkGenerator.GetUriByName(context, confirmEmailEndpointName, routeValues)
                    ?? throw new NotSupportedException($"Could not find endpoint named '{confirmEmailEndpointName}'.");

                await emailSender.SendConfirmationLinkAsync(user, email, HtmlEncoder.Default.Encode(confirmEmailUrl));
            }
        }

        // 生成驗證碼圖片
        routeGroup.MapGet("/captcha", (ICaptchaService captchaService) =>
        {
            var captchaInfo = captchaService.Create(2);

            string base64 = Convert.ToBase64String(captchaInfo.Image, 0, captchaInfo.Image.Length);

            return Results.Ok(base64);
        }).WithName("Captcha").AllowAnonymous();


        // 登入並取得 Jwt Token
        routeGroup.MapPost("/signin", async ([FromBody] LoginDto login, JwtManager jwtManager, ICaptchaService captchaService, IIdentityManager identityManager) =>
        {
            if (login != null)
            {
                if (!captchaService.Verify(login.Captcha))
                {
                    return Results.Problem("驗證碼錯誤", statusCode: StatusCodes.Status401Unauthorized);
                }

                var validateUser = await identityManager.ValidateUserAndSignInAsync(login);

                if (validateUser)
                {
                    var accessToken = jwtManager.GenerateToken(login.UserName);
                    var refreshToken = jwtManager.GenerateToken(login.UserName, 240);
                    return Results.Ok(new { accessToken, refreshToken });
                }
            }

            return Results.BadRequest();
        }).WithName("SignIn").AllowAnonymous();

        // 刷新 Jwt Token
        routeGroup.MapGet("/refresh", (string token, ClaimsPrincipal user, JwtManager jwtManager) =>
        {
            var userName = user.Identity?.Name;
            if (!string.IsNullOrEmpty(userName) && jwtManager.VerifyToken(token))
            {
                var accessToken = jwtManager.GenerateToken(userName);
                var refreshToken = jwtManager.GenerateToken(userName, 240);
                return Results.Ok(new { accessToken, refreshToken });
            }
            return Results.Unauthorized();
        }).WithName("Refresh").RequireAuthorization();

        // 登出並注銷 Jwt Token
        routeGroup.MapPost("/signout", async (SignInManager<IdentityUser> signInManager, JwtManager jwtManager) =>
        {
            await signInManager.SignOutAsync();
            jwtManager.RemoveToken();
        }).WithName("SignOut").RequireAuthorization();

        //// 取得 JWT Token 中的所有 Claims
        //routeGroup.MapGet("/claims", (ClaimsPrincipal user) =>
        //{
        //    return Results.Ok(user.Claims.Select(p => new { p.Type, p.Value }));
        //}).WithName("Claims").RequireAuthorization();

        //// 取得 JWT Token 中的使用者名稱
        //routeGroup.MapGet("/username", (ClaimsPrincipal user) =>
        //{
        //    return Results.Ok(user.Identity?.Name);
        //}).WithName("Username").RequireAuthorization();

        //// 取得使用者是否擁有特定角色
        //routeGroup.MapGet("/isInRole", (ClaimsPrincipal user, string name) =>
        //{
        //    return Results.Ok(user.IsInRole(name));
        //}).WithName("IsInRole").RequireAuthorization();

        //// 取得 JWT Token 中的 JWT ID
        //routeGroup.MapGet("/jwtid", (ClaimsPrincipal user) =>
        //{
        //    return Results.Ok(user.Claims.FirstOrDefault(p => p.Type == "jti")?.Value);
        //}).WithName("JwtId").RequireAuthorization();

        //routeGroup.MapGet("/signout", async (SignInManager<IdentityUser> signInManager) =>
        //{
        //    await signInManager.SignOutAsync();
        //    return Results.Ok();
        //}).WithName("SignOut").RequireAuthorization();

        return new IdentityEndpointsConventionBuilder(routeGroup);
    }

    private sealed class IdentityEndpointsConventionBuilder(RouteGroupBuilder inner) : IEndpointConventionBuilder
    {
        private IEndpointConventionBuilder InnerAsConventionBuilder => inner;

        public void Add(Action<EndpointBuilder> convention) => InnerAsConventionBuilder.Add(convention);
    }

    private static ValidationProblem CreateValidationProblem(string errorCode, string errorDescription) =>
    TypedResults.ValidationProblem(new Dictionary<string, string[]> {
            { errorCode, [errorDescription] }
    });

    private static ValidationProblem CreateValidationProblem(IdentityResult result)
    {
        // We expect a single error code and description in the normal case.
        // This could be golfed with GroupBy and ToDictionary, but perf! :P
        Debug.Assert(!result.Succeeded);
        var errorDictionary = new Dictionary<string, string[]>(1);

        foreach (var error in result.Errors)
        {
            string[] newDescriptions;

            if (errorDictionary.TryGetValue(error.Code, out var descriptions))
            {
                newDescriptions = new string[descriptions.Length + 1];
                Array.Copy(descriptions, newDescriptions, descriptions.Length);
                newDescriptions[descriptions.Length] = error.Description;
            }
            else
            {
                newDescriptions = [error.Description];
            }

            errorDictionary[error.Code] = newDescriptions;
        }

        return TypedResults.ValidationProblem(errorDictionary);
    }

}
