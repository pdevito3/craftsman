namespace Craftsman.Builders.Seeders
{
    using Craftsman.Helpers;
    using Craftsman.Models;
    using System;
    using Enums;

    public static class SeederFunctions
    {
        public static string GetEntitySeederFileText(string classNamespace, Entity entity, string dbContextName, string solutionDirectory, string projectBaseName)
        {
            var entitiesClassPath = ClassPathHelper.EntityClassPath(solutionDirectory, "", entity.Plural, projectBaseName);
            var dtoClassPath = ClassPathHelper.DtoClassPath(solutionDirectory, "", entity.Name);
            var dbContextClassPath = ClassPathHelper.DbContextClassPath(solutionDirectory, "", projectBaseName);
            var entityForCreationDto = $"{Utilities.GetDtoName(entity.Name, Dto.Creation)}";
            if (dbContextName is null)
            {
                throw new ArgumentNullException(nameof(dbContextName));
            }

            return @$"namespace {classNamespace};

using AutoBogus;
using {entitiesClassPath.ClassNamespace};
using {dtoClassPath.ClassNamespace};
using {dbContextClassPath.ClassNamespace};
using System.Linq;

public static class {Utilities.GetSeederName(entity)}
{{
    public static void SeedSample{entity.Name}Data({dbContextName} context)
    {{
        if (!context.{entity.Plural}.Any())
        {{
            context.{entity.Plural}.Add({entity.Name}.Create(new AutoFaker<{entityForCreationDto}>()));
            context.{entity.Plural}.Add({entity.Name}.Create(new AutoFaker<{entityForCreationDto}>()));
            context.{entity.Plural}.Add({entity.Name}.Create(new AutoFaker<{entityForCreationDto}>()));

            context.SaveChanges();
        }}
    }}
}}";
        }
    }
}
