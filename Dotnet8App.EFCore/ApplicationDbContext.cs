using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;

namespace Dotnet8App.EFCore;

public class ApplicationDbContext(DbContextOptions options) : IdentityDbContext(options)
{
}
