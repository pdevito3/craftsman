namespace Craftsman.Builders;

using Domain;
using Helpers;
using MediatR;
using Services;

public static class DatabaseEntityConfigRolePermissionBuilder
{
    public class Command : IRequest<bool>
    {
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
            var entityName = "RolePermission";
            var classPath = ClassPathHelper.DatabaseConfigClassPath(_scaffoldingDirectoryStore.SrcDirectory, 
                $"{FileNames.GetDatabaseEntityConfigName(entityName)}.cs",
                _scaffoldingDirectoryStore.ProjectBaseName);
            var fileText = GetFileText(classPath.ClassNamespace, entityName);
            _utilities.CreateFile(classPath, fileText);
            return Task.FromResult(true);
        }
        private string GetFileText(string classNamespace, string entityName)
        {
            return @$"namespace {classNamespace};

using Domain.RolePermissions;
using Domain.Roles;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

public sealed class {FileNames.GetDatabaseEntityConfigName(entityName)} : IEntityTypeConfiguration<{entityName}>
{{
    /// <summary>
    /// The database configuration for RolePermissions. 
    /// </summary>
    public void Configure(EntityTypeBuilder<RolePermission> builder)
    {{
        // Relationship Marker -- Deleting or modifying this comment could cause incomplete relationship scaffolding

        builder.ComplexProperty(x => x.Role,
            y => y.Property(z => z.Value)
                .HasColumnName(""role""));
    }}
}}";
        }
    }
    
}
