namespace Craftsman.Builders;

using Helpers;
using MediatR;
using Services;

public static class GenericRepositoryBuilder
{
    public class GenericRepositoryBuilderCommand : IRequest<bool>
    {
        public readonly string DbContextName;

        public GenericRepositoryBuilderCommand(string dbContextName)
        {
            DbContextName = dbContextName;
        }
    }

    public class Handler : IRequestHandler<GenericRepositoryBuilderCommand, bool>
    {
        private readonly ICraftsmanUtilities _utilities;
        private readonly IScaffoldingDirectoryStore _scaffoldingDirectoryStore;

        public Handler(ICraftsmanUtilities utilities,
            IScaffoldingDirectoryStore scaffoldingDirectoryStore)
        {
            _utilities = utilities;
            _scaffoldingDirectoryStore = scaffoldingDirectoryStore;
        }

        public Task<bool> Handle(GenericRepositoryBuilderCommand request, CancellationToken cancellationToken)
        {
            var classPath = ClassPathHelper.WebApiServicesClassPath(_scaffoldingDirectoryStore.SrcDirectory, $"{FileNames.GenericRepository()}.cs", _scaffoldingDirectoryStore.ProjectBaseName);
            var fileText = GetFileText(classPath.ClassNamespace, request.DbContextName);
            _utilities.CreateFile(classPath, fileText);
            return Task.FromResult(true);
        }

        private string GetFileText(string classNamespace, string dbContextName)
        {
            var boundaryServiceName = FileNames.BoundaryServiceInterface(_scaffoldingDirectoryStore.ProjectBaseName);
            var contextClassPath = ClassPathHelper.DbContextClassPath(_scaffoldingDirectoryStore.SrcDirectory, "", _scaffoldingDirectoryStore.ProjectBaseName);
            var domainClassPath = ClassPathHelper.EntityClassPath(_scaffoldingDirectoryStore.SrcDirectory, "", "", _scaffoldingDirectoryStore.ProjectBaseName);
            var exceptionsClassPath = ClassPathHelper.ExceptionsClassPath(_scaffoldingDirectoryStore.SolutionDirectory, _scaffoldingDirectoryStore.ProjectBaseName);

            var repoName = FileNames.GenericRepository();
            var interfaceName = FileNames.GenericRepositoryInterface();

            return @$"namespace {classNamespace};

using {domainClassPath.ClassNamespace};
using {contextClassPath.ClassNamespace};
using {exceptionsClassPath.ClassNamespace};
using Microsoft.EntityFrameworkCore;

public interface {interfaceName}<TEntity> : {boundaryServiceName}
    where TEntity : BaseEntity
{{
    IQueryable<TEntity> Query();
    Task<TEntity> GetByIdOrDefault(int id, bool withTracking = true, CancellationToken cancellationToken = default);
    Task<TEntity> GetById(int id, bool withTracking = true, CancellationToken cancellationToken = default);
    Task<bool> Exists(int id, CancellationToken cancellationToken = default);
    Task Add(TEntity entity, CancellationToken cancellationToken = default);    
    Task AddRange(IEnumerable<TEntity> entity, CancellationToken cancellationToken = default);    
    void Update(TEntity entity);
    void Remove(TEntity entity);
    void RemoveRange(IEnumerable<TEntity> entity);
}}

public abstract class {repoName}<TEntity> : {interfaceName}<TEntity> 
    where TEntity : BaseEntity
{{
    private readonly {dbContextName} _dbContext;

    protected {repoName}({dbContextName} dbContext)
    {{
        this._dbContext = dbContext;
    }}
    
    public virtual IQueryable<TEntity> Query()
    {{
        return _dbContext.Set<TEntity>();
    }}

    public virtual async Task<TEntity> GetByIdOrDefault(int id, bool withTracking = true, CancellationToken cancellationToken = default)
    {{
        return withTracking 
            ? await _dbContext.Set<TEntity>()
                .FirstOrDefaultAsync(e => e.Id == id, cancellationToken) 
            : await _dbContext.Set<TEntity>()
                .AsNoTracking()
                .FirstOrDefaultAsync(e => e.Id == id, cancellationToken);
    }}

    public virtual async Task<TEntity> GetById(int id, bool withTracking = true, CancellationToken cancellationToken = default)
    {{
        var entity = await GetByIdOrDefault(id, withTracking, cancellationToken);
        
        if(entity == null)
            throw new NotFoundException($""{{typeof(TEntity).Name}} with an id '{{id}}' was not found."");

        return entity;
    }}

    public virtual async Task<bool> Exists(int id, CancellationToken cancellationToken = default)
    {{
        return await _dbContext.Set<TEntity>()
            .AnyAsync(e => e.Id == id, cancellationToken);
    }}

    public virtual async Task Add(TEntity entity, CancellationToken cancellationToken = default)
    {{
        await _dbContext.Set<TEntity>().AddAsync(entity, cancellationToken);
    }}

    public virtual async Task AddRange(IEnumerable<TEntity> entities, CancellationToken cancellationToken = default)
    {{
        await _dbContext.Set<TEntity>().AddRangeAsync(entities, cancellationToken);
    }}

    public virtual void Update(TEntity entity)
    {{
        _dbContext.Set<TEntity>().Update(entity);
    }}

    public virtual void Remove(TEntity entity)
    {{
        _dbContext.Set<TEntity>().Remove(entity);
    }}

    public virtual void RemoveRange(IEnumerable<TEntity> entities)
    {{
        _dbContext.Set<TEntity>().RemoveRange(entities);
    }}
}}
";
        }
    }
}
