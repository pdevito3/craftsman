namespace Craftsman.Builders.Seeders
{
    using Craftsman.Helpers;
    using Craftsman.Models;
    using System;
    using System.Collections.Generic;
    using System.Linq;

    public static class SeederFunctions
    {
        public static string GetEntitySeederFileText(string classNamespace, Entity entity, string dbContextName)
        {
            if (dbContextName is null)
            {
                throw new ArgumentNullException(nameof(dbContextName));
            }

            return @$"namespace {classNamespace}
{{

    using AutoBogus;
    using Domain.Entities;
    using Infrastructure.Persistence.Contexts;
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

        public static string GetIdentitySeederFileText(string classNamespace, ApplicationUser user)
        {
            var seederName = Utilities.GetIdentitySeederName(user);
            var roleString = "";

            foreach (var role in user.AuthorizedRoles)
            {
                var newLine = role == user.AuthorizedRoles.LastOrDefault() ? "" : $"{Environment.NewLine}                    ";
                roleString += @$"await userManager.AddToRoleAsync(defaultUser, Role.{role}.ToString());{newLine}";
            }

            return @$"namespace {classNamespace}
{{
    using Bogus;
    using Domain.Enums;
    using Infrastructure.Identity.Entities;
    using Microsoft.AspNetCore.Identity;
    using System.Linq;
    using System.Threading.Tasks;

    public static class {seederName}
    {{
        public static async Task SeedUserAsync(UserManager<ApplicationUser> userManager)
        {{
            var defaultUser = new ApplicationUser
            {{
                UserName = $""{user.UserName}"",
                Email = ""{user.Email}"",
                FirstName = ""{user.FirstName}"",
                LastName = ""{user.LastName}"",
                EmailConfirmed = true,
                PhoneNumberConfirmed = true
            }};

            if (userManager.Users.All(u => u.Id != defaultUser.Id))
            {{
                var user = await userManager.FindByEmailAsync(defaultUser.Email);
                if (user == null)
                {{
                    await userManager.CreateAsync(defaultUser, ""{user.Password}"");
                    {roleString}
                }}
            }}
        }}
    }}
}}";
        }
    }
}
