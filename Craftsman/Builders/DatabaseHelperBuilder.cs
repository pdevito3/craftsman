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

        public Command(string dbContext)
        {
            DbContext = dbContext;
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
            var fileText = GetFileText(classPath.ClassNamespace, request.DbContext);
            _utilities.CreateFile(classPath, fileText);
            return Task.FromResult(true);
        }
        private string GetFileText(string classNamespace, string dbContext)
        {
            return @$"namespace {classNamespace};


using Microsoft.EntityFrameworkCore;

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
