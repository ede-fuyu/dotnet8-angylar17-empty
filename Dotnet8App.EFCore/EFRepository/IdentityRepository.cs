using Microsoft.AspNetCore.Identity;

namespace Dotnet8App.EFCore
{
    public class IdentityRepository(ApplicationDbContext context) : Repository<IdentityUser>(context), IIdentityRepository
    {
        public IdentityUser? FindUserByName(string userName)
        {
            var identityUser = this.GetAll().Where(p => p.UserName == userName).FirstOrDefault();
            return identityUser;
        }
    }

    public interface IIdentityRepository
    {
        IdentityUser? FindUserByName(string userName);
    }
}
