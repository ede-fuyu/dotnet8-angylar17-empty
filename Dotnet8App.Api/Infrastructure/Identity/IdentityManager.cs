using Microsoft.AspNetCore.Identity;

namespace Dotnet8App.Api.Infrastructure
{
    public class IdentityManager(SignInManager<IdentityUser> signInManager, UserManager<IdentityUser> userManager) : IIdentityManager
    {
        public async Task<bool> ValidateUserAndSignInAsync(LoginDto login)
        {
            var user = userManager.Users.Where(p => p.UserName == login.UserName).FirstOrDefault();
            if (user != null)
            {
                var signInResult = await signInManager.CheckPasswordSignInAsync(user, login.Password, true);
                return signInResult.Succeeded;
            }

            return false;
        }
    }

    public interface IIdentityManager
    {
        Task<bool> ValidateUserAndSignInAsync(LoginDto login);
    }
}
