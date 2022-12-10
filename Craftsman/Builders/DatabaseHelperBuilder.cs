namespace Craftsman.Builders;

using Domain;
using Helpers;
using MediatR;
using Services;

public static class DatabaseHelperBuilder
{
    public class Command : IRequest<bool>
    {
        public readonly string DbContext;
        public readonly DbProvider DbProvider;

        public Command(string dbContext, DbProvider dbProvider)
        {
            DbContext = dbContext;
            DbProvider = dbProvider;
        }
    }

    public class Handler : IRequestHandler<Command, bool>
    {
        private readonly ICraftsmanUtilities _utilities;
        private readonly IScaffoldingDirectoryStore _scaffoldingDirectoryStore;

        public Handler(ICraftsmanUtilities utilities,
            IScaffoldingDirectoryStore scaffoldingDirectoryStore)
        {
            _utilities = utilities;
            _scaffoldingDirectoryStore = scaffoldingDirectoryStore;
        }

        public Task<bool> Handle(Command request, CancellationToken cancellationToken)
        {
            var classPath = ClassPathHelper.DbContextClassPath(_scaffoldingDirectoryStore.SrcDirectory, 
                $"{FileNames.GetDatabaseHelperFileName()}.cs",
                _scaffoldingDirectoryStore.ProjectBaseName);
            var fileText = GetFileText(classPath.ClassNamespace, request.DbContext, request.DbProvider);
            _utilities.CreateFile(classPath, fileText);
            return Task.FromResult(true);
        }
        private string GetFileText(string classNamespace, string dbContext, DbProvider dbProvider)
        {
            var usingStatement = dbProvider == DbProvider.Postgres ? $@"
using Npgsql;" : "";
            var catchStatement = dbProvider == DbProvider.Postgres 
                ? $@"catch (Exception ex) when (ex is SocketException or NpgsqlException)"
                : $@"catch (Exception ex) when (ex is SocketException)";
            return @$"namespace {classNamespace};

using Microsoft.EntityFrameworkCore;
using System.Net.Sockets;{usingStatement}

public sealed class {FileNames.GetDatabaseHelperFileName()}
{{
    private readonly ILogger<{FileNames.GetDatabaseHelperFileName()}> _logger;
    private readonly {dbContext} _context;

    public {FileNames.GetDatabaseHelperFileName()}(ILogger<DatabaseHelper> logger, {dbContext} context)
    {{
        _logger = logger;
        _context = context;
    }}

    public async Task MigrateAsync()
    {{
        try
        {{
            await _context.Database.MigrateAsync();
        }}
        {catchStatement}
        {{
            _logger.LogError(ex, ""Could not connect to the database. Please check the connection string and make sure the database is running."");
            throw;
        }}
        catch (Exception ex)
        {{
            _logger.LogError(ex, ""An error occurred while applying the database migrations."");
            throw;
        }}
    }}

    public async Task SeedAsync()
    {{
        try
        {{
            await TrySeedAsync();
        }}
        catch (Exception ex)
        {{
            _logger.LogError(ex, ""An error occurred while seeding the database."");
            throw;
        }}
    }}

    public async Task TrySeedAsync()
    {{
        // Seed base data, if you want
    }}
}}";
        }
    }
    
}
