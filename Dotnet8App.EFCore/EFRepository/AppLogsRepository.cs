using Dotnet8App.EFCore.EntityTables;
using System.Collections.Generic;
using System.Threading.Tasks;
using System.Text;

namespace Dotnet8App.EFCore;

public class AppLogsRepository(ApplicationDbContext context) : Repository<AppLogs>(context), IAppLogsRepository
{

}

public interface IAppLogsRepository : IRepository<AppLogs>
{

}

