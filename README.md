# dotnet8 Web Api and angylar17 empty

[serilog-sinks-mssqlserver]: https://github.com/serilog-mssql/serilog-sinks-mssqlserver
## AppLogs Table Definition from [serilog-sinks-mssqlserver]
    CREATE TABLE [AppLogs] (
    
       [Id] int IDENTITY(1,1) NOT NULL,
       [UserName] nvarchar(256) NULL,
       [RequestUri] nvarchar(max) NULL,
       [StatusCodes] int NULL,
       [Message] nvarchar(max) NULL,
       [MessageTemplate] nvarchar(max) NULL,
       [Level] nvarchar(128) NULL,
       [TimeStamp] datetime NOT NULL,
       [Exception] nvarchar(max) NULL,
       [Properties] nvarchar(max) NULL
    
       CONSTRAINT [PK_AppLogs] PRIMARY KEY CLUSTERED ([Id] ASC)
    );
