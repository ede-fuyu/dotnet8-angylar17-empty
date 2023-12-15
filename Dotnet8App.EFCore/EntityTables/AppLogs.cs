using System;

namespace Dotnet8App.EFCore.EntityTables;

public partial class AppLogs
{
    public AppLogs()
    {
    }

    public int Id { get; set; }

    public string? UserName { get; set; }

    public string? RequestUri { get; set; }

    public int? StatusCodes { get; set; }

    public string? Message { get; set; }

    public string? MessageTemplate { get; set; }

    public string? Level { get; set; }

    public DateTime TimeStamp { get; set; }

    public string? Exception { get; set; }

    public string? Properties { get; set; }
}
