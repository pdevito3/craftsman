namespace Craftsman.Builders;

using Helpers;
using MediatR;
using Services;

public static class UserEntityRepositoryBuilder
{
    public class Command : IRequest<bool>
    {
        public readonly string DbContextName;
        public readonly string EntityName;
        public readonly string EntityPlural;

        public readonly bool Overwrite;
        
        public Command(string dbContextName, string entityName, string entityPlural, bool overwrite)
        {
            DbContextName = dbContextName;
            EntityPlural = entityPlural;
            EntityName = entityName;
            Overwrite = overwrite;
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
            var classPath = ClassPathHelper.EntityServicesClassPath(_scaffoldingDirectoryStore.SrcDirectory, 
                $"{FileNames.EntityRepository(request.EntityName)}.cs", 
                request.EntityPlural, 
                _scaffoldingDirectoryStore.ProjectBaseName);
            var fileText = GetFileText(classPath.ClassNamespace, request.EntityName, request.EntityPlural, request.DbContextName);
            _utilities.CreateFile(classPath, fileText, request.Overwrite);
            return Task.FromResult(true);
        }

        private string GetFileText(string classNamespace, string entityName, string entityPlural, string dbContextName)
        {
            var entityClassPath = ClassPathHelper.EntityClassPath(_scaffoldingDirectoryStore.SrcDirectory, 
                "", 
                entityPlural, 
                _scaffoldingDirectoryStore.ProjectBaseName);
            var contextClassPath = ClassPathHelper.DbContextClassPath(_scaffoldingDirectoryStore.SrcDirectory, 
                "", 
                _scaffoldingDirectoryStore.ProjectBaseName);
            var servicesClassPath = ClassPathHelper.WebApiServicesClassPath(_scaffoldingDirectoryStore.SrcDirectory,
                "", 
                _scaffoldingDirectoryStore.ProjectBaseName);

            var genericRepositoryInterface = FileNames.GenericRepositoryInterface();
            var genericRepoName = FileNames.GenericRepository();
            var repoInterface = FileNames.EntityRepositoryInterface(entityName);
            var repoName = FileNames.EntityRepository(entityName);
            
            return @$"namespace {classNamespace};

using Microsoft.EntityFrameworkCore;
using {entityClassPath.ClassNamespace};
using {contextClassPath.ClassNamespace};
using {servicesClassPath.ClassNamespace};

public interface {repoInterface} : {genericRepositoryInterface}<{entityName}>
{{
    public IEnumerable<string> GetRolesByUserIdentifier(string identifier);
    public Task AddRole(UserRole entity, CancellationToken cancellationToken = default);
    public void RemoveRole(UserRole entity);
}}

public class {repoName} : {genericRepoName}<{entityName}>, {repoInterface}
{{
    private readonly {dbContextName} _dbContext;

    public {repoName}({dbContextName} dbContext) : base(dbContext)
    {{
        _dbContext = dbContext;
    }}

    public override async Task<User> GetByIdOrDefault(Guid id, bool withTracking = true, CancellationToken cancellationToken = default)
    {{
        return withTracking 
            ? await _dbContext.Users
                .Include(u => u.Roles)
                .FirstOrDefaultAsync(e => e.Id == id, cancellationToken) 
            : await _dbContext.Users
                .Include(u => u.Roles)
                .AsNoTracking()
                .FirstOrDefaultAsync(e => e.Id == id, cancellationToken);
    }}

    public async Task AddRole(UserRole userRole, CancellationToken cancellationToken = default)
    {{
        await _dbContext.UserRoles.AddAsync(userRole, cancellationToken);
    }}

    public void RemoveRole(UserRole userRole)
    {{
        _dbContext.UserRoles.Remove(userRole);
    }}

    public IEnumerable<string> GetRolesByUserIdentifier(string identifier)
    {{
        return _dbContext.UserRoles
            .Include(x => x.User)
            .Where(x => x.User.Identifier == identifier)
            .Select(x => x.Role.Value);
    }}
}}
";
        }
    }
}
