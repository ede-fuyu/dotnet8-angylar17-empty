using Dotnet8App.EFCore;
using Dotnet8App.EFCore.EntityTables;

namespace Dotnet8App.Service
{
    public class AppLogService(IRepository<AppLogs> logRepo) : IAppLogService
    {
        public List<AppLogs> GetAppLogs()
        {
            var data = logRepo.GetAll().ToList();
            return data;
        }
    }

    public interface IAppLogService
    {
        List<AppLogs> GetAppLogs();
    }
}
