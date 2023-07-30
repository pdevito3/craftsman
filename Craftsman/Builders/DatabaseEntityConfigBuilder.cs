namespace Craftsman.Builders;

using Domain;
using Helpers;
using MediatR;
using Services;

public static class DatabaseEntityConfigBuilder
{

    public record Command(string EntityName, string EntityPlural, List<EntityProperty> Properties) : IRequest<bool>;
    
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
            var classPath = ClassPathHelper.DatabaseConfigClassPath(_scaffoldingDirectoryStore.SrcDirectory, 
                $"{FileNames.GetDatabaseEntityConfigName(request.EntityName)}.cs",
                _scaffoldingDirectoryStore.ProjectBaseName);
            var fileText = GetFileText(classPath.ClassNamespace, request.EntityName, request.EntityPlural, request.Properties);
            _utilities.CreateFile(classPath, fileText);
            return Task.FromResult(true);
        }
        private string GetFileText(string classNamespace, string entityName, string entityPlural, List<EntityProperty> properties)
        {
            var domainPolicyClassPath = ClassPathHelper.EntityClassPath(_scaffoldingDirectoryStore.SrcDirectory,
                "", 
                entityPlural, 
                _scaffoldingDirectoryStore.ProjectBaseName);
            
            var relationshipConfigs = string.Empty;
            foreach (var entityProperty in properties.Where(x => x.Relationship == "1tomany"))
            {
                relationshipConfigs += @$"{Environment.NewLine}        builder.HasMany(x => x.{entityProperty.Name})
            .WithOne(x => x.{entityName});";
            }
            foreach (var entityProperty in properties.Where(x => x.Relationship == "1to1"))
            {
                relationshipConfigs += @$"{Environment.NewLine}        builder.HasOne(x => x.{entityProperty.Name})
            .WithOne(x => x.{entityName})
            .HasForeignKey<{entityName}>(s => s.Id);";
            }
            foreach (var entityProperty in properties.Where(x => x.Relationship == "manytomany"))
            {
                relationshipConfigs += @$"{Environment.NewLine}        builder.HasMany(x => x.{entityProperty.Name})
            .WithMany(x => x.{entityPlural});";
            }
            foreach (var entityProperty in properties.Where(x => x.Relationship == "manyto1"))
            {
                relationshipConfigs += @$"{Environment.NewLine}        builder.HasOne(x => x.{entityProperty.Name})
            .WithMany(x => x.{entityPlural});";
            }
            relationshipConfigs += string.IsNullOrWhiteSpace(relationshipConfigs) ? string.Empty : $"{Environment.NewLine}";
            
            return @$"namespace {classNamespace};

using {domainPolicyClassPath.ClassNamespace};
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

public sealed class {FileNames.GetDatabaseEntityConfigName(entityName)} : IEntityTypeConfiguration<{entityName}>
{{
    /// <summary>
    /// The database configuration for {entityPlural}. 
    /// </summary>
    public void Configure(EntityTypeBuilder<{entityName}> builder)
    {{{relationshipConfigs}
        // example for a simple 1:1 value object
        // builder.Property(x => x.Percent)
        //     .HasConversion(x => x.Value, x => new Percent(x))
        //     .HasColumnName(""percent"");
        
        // example for a more complex value object
        // builder.OwnsOne(x => x.PhysicalAddress, opts =>
        // {{
        //     opts.Property(x => x.Line1).HasColumnName(""physical_address_line1"");
        //     opts.Property(x => x.Line2).HasColumnName(""physical_address_line2"");
        //     opts.Property(x => x.City).HasColumnName(""physical_address_city"");
        //     opts.Property(x => x.State).HasColumnName(""physical_address_state"");
        //     opts.Property(x => x.PostalCode).HasColumnName(""physical_address_postal_code"")
        //         .HasConversion(x => x.Value, x => new PostalCode(x));
        //     opts.Property(x => x.Country).HasColumnName(""physical_address_country"");
        // }}).Navigation(x => x.PhysicalAddress)
        //     .IsRequired();
    }}
}}";
        }
    }
    
}
