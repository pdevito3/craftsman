namespace Craftsman.Builders
{
    using Craftsman.Builders.Dtos;
    using Craftsman.Enums;
    using Craftsman.Exceptions;
    using Craftsman.Helpers;
    using Craftsman.Models;
    using System;
    using System.Collections.Generic;
    using System.IO;
    using System.Linq;
    using System.Reflection.Emit;
    using System.Text;
    using static Helpers.ConsoleWriter;

    public class DbContextBuilder
    {
        public static void CreateDbContext(string solutionDirectory, ApiTemplate template)
        {
            try
            {
                //TODO move these to a dictionary to lookup and overwrite if I want
                var entityTopPath = "Infrastructure.Persistence\\Contexts";
                var entityNamespace = entityTopPath.Replace("\\", ".");

                var entityDir = Path.Combine(solutionDirectory, entityTopPath);
                if (!Directory.Exists(entityDir))
                    Directory.CreateDirectory(entityDir);

                var pathString = Path.Combine(entityDir, $"{template.DbContext.ContextName}.cs");
                if (File.Exists(pathString))
                    throw new FileAlreadyExistsException(pathString);

                using (FileStream fs = File.Create(pathString))
                {
                    var data = GetControllerFileText(entityNamespace, template);
                    fs.Write(Encoding.UTF8.GetBytes(data));
                }

                WriteInfo($"A new '{template.DbContext.ContextName}' DbContext file was added here: {pathString}.");
            }
            catch (FileAlreadyExistsException)
            {
                WriteError("This file alread exists. Please enter a valid file path.");
                throw;
            }
            catch (Exception e)
            {
                WriteError($"An unhandled exception occured when running the API command.\nThe error details are: \n{e.Message}");
                throw;
            }
        }

        public static string GetControllerFileText(string classNamespace, ApiTemplate template)
        {
            return @$"namespace {classNamespace}
{{

    using Application.Interfaces;
    using Domain.Common;
    using Domain.Entities;
    using Microsoft.EntityFrameworkCore;
    using Microsoft.EntityFrameworkCore.ChangeTracking;
    using System.Threading;
    using System.Threading.Tasks;

    public class {template.DbContext.ContextName} : DbContext
    {{
        private readonly IDateTimeService _dateTimeService;

        public {template.DbContext.ContextName}(
            DbContextOptions<{template.DbContext.ContextName}> options,
            IDateTimeService dateTime) : base(options) 
        {{
            _dateTimeService = dateTime;
        }}

{GetDbSetText(template.Entities)}

        //TODO: Abstract this logic out into an custom inheritable dbcontext
        public override Task<int> SaveChangesAsync(CancellationToken cancellationToken = new CancellationToken())
        {{
            foreach (var entry in ChangeTracker.Entries<AuditableEntity>())
            {{
                UpdateAuditableFields(entry, _dateTimeService);
            }}

            return base.SaveChangesAsync(cancellationToken);
        }}

        public override int SaveChanges()
        {{
            foreach (var entry in ChangeTracker.Entries<AuditableEntity>())
            {{
                UpdateAuditableFields(entry, _dateTimeService);
            }}

            return base.SaveChanges();
        }}

        public static void UpdateAuditableFields(EntityEntry<AuditableEntity> entry, IDateTimeService service)
        {{
            switch (entry.State)
            {{
                case EntityState.Added:
                    entry.Entity.CreatedBy = ""TBD User""; //_currentUserService.UserId;
                    entry.Entity.CreatedOn = service.NowUtc;
                    break;
                case EntityState.Modified:
                    entry.Entity.LastModifiedBy = ""TBD User""; //_currentUserService.UserId;
                    entry.Entity.LastModifiedOn = service.NowUtc;
                    break;
            }}            
        }}
    }}
}}";
        }

        public static string GetDbSetText(List<Entity> entities)
        {
            var dbSetText = "";

            foreach(var entity in entities)
            {
                string newLine = entity == entities.LastOrDefault() ? "" : $"{Environment.NewLine}";
                dbSetText += @$"        public DbSet<{entity.Name}> {entity.Plural} {{ get; set; }}{newLine}";
            }

            return dbSetText;
        }
    }
}
