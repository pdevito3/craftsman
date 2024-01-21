namespace Craftsman.Builders;

using Domain;
using Helpers;
using MediatR;
using Services;

public static class DbMigrationsHostedServiceBuilder
{
    public class Command : IRequest<bool>
    {
        public readonly DbProvider DbProvider;

        public Command(DbProvider dbProvider)
        {
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
                $"{FileNames.GetMigrationHostedServiceFileName()}.cs",
                _scaffoldingDirectoryStore.ProjectBaseName);
            var fileText = GetFileText(classPath.ClassNamespace, request.DbProvider);
            _utilities.CreateFile(classPath, fileText);
            return Task.FromResult(true);
        }
        private string GetFileText(string classNamespace, DbProvider dbProvider)
        {
            var usingStatement = dbProvider == DbProvider.Postgres ? $@"
using Npgsql;" : "";
            var catchStatement = dbProvider == DbProvider.Postgres 
                ? $@"catch (Exception ex) when (ex is SocketException or NpgsqlException)"
                : $@"catch (Exception ex) when (ex is SocketException)";
            return @$"namespace {classNamespace};

using System.Net.Sockets;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Serilog;{usingStatement}

public class {FileNames.GetMigrationHostedServiceFileName()}<TDbContext>(
    IServiceScopeFactory scopeFactory)
    : IHostedService
    where TDbContext : DbContext
{{
    private readonly ILogger _logger = Log.ForContext<{FileNames.GetMigrationHostedServiceFileName()}<TDbContext>>();

    public async Task StartAsync(CancellationToken cancellationToken)
    {{
        try
        {{
            _logger.Information(""Applying migrations for {{DbContext}}"", typeof(TDbContext).Name);

            await using var scope = scopeFactory.CreateAsyncScope();
            var context = scope.ServiceProvider.GetRequiredService<TDbContext>();
            await context.Database.MigrateAsync(cancellationToken);

            _logger.Information(""Migrations complete for {{DbContext}}"", typeof(TDbContext).Name);
        }}
        {catchStatement}
        {{
            _logger.Error(ex, ""Could not connect to the database. Please check the connection string and make sure the database is running."");
            throw;
        }}
        catch (Exception ex)
        {{
            _logger.Error(ex, ""An error occurred while applying the database migrations."");
            throw;
        }}
    }}

    public Task StopAsync(CancellationToken cancellationToken)
    {{
        return Task.CompletedTask;
    }}
}}";
        }
    }
    
}
