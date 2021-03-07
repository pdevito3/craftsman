namespace Craftsman.Builders.Seeders
{
    using Craftsman.Helpers;
    using Craftsman.Models;
    using System;
    using System.Collections.Generic;
    using System.Linq;

    public static class SeederFunctions
    {
        public static string GetEntitySeederFileText(string classNamespace, Entity entity, string dbContextName, string solutionDirectory, string projectBaseName)
        {
            var entitiesClassPath = ClassPathHelper.EntityClassPath(solutionDirectory, "", projectBaseName);
            var dbContextClassPath = ClassPathHelper.DbContextClassPath(solutionDirectory, "", projectBaseName);
            if (dbContextName is null)
            {
                throw new ArgumentNullException(nameof(dbContextName));
            }

            return @$"namespace {classNamespace}
{{

    using AutoBogus;
    using {entitiesClassPath.ClassNamespace};
    using {dbContextClassPath.ClassNamespace};
    using System.Linq;

    public static class {Utilities.GetSeederName(entity)}
    {{
        public static void SeedSample{entity.Name}Data({dbContextName} context)
        {{
            if (!context.{entity.Plural}.Any())
            {{
                context.{entity.Plural}.Add(new AutoFaker<{entity.Name}>());
                context.{entity.Plural}.Add(new AutoFaker<{entity.Name}>());
                context.{entity.Plural}.Add(new AutoFaker<{entity.Name}>());

                context.SaveChanges();
            }}
        }}
    }}
}}";
        }
    }
}
