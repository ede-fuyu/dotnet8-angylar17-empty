using Dotnet8App.EFCore;
using Microsoft.AspNetCore.Identity;

namespace Dotnet8App.Service
{
    public class IdentityService(IIdentityRepository userRepository) : IIdentityService
    {
        public IdentityUser? GetUser(string userName)
        {
            return userRepository.FindUserByName(userName);
        }
    }

    public interface IIdentityService
    {
        IdentityUser? GetUser(string userName);
    }
}
