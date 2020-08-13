namespace Craftsman.Builders.Seeders
{
    using Craftsman.Helpers;
    using Craftsman.Models;
    using System;
    using System.Collections.Generic;

    public static class SeederFunctions
    {
        public static string GetSeederFileText(string classNamespace, Entity entity, ApiTemplate template)
        {
            if (template is null)
            {
                throw new ArgumentNullException(nameof(template));
            }

            return @$"namespace {classNamespace}
{{

    using AutoBogus;
    using Domain.Entities;
    using Infrastructure.Persistence.Contexts;
    using System.Linq;

    public static class {Utilities.GetSeederName(entity)}
    {{
        public static void SeedSample{entity.Name}Data({template.DbContext.ContextName} context)
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
