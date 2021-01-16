namespace Craftsman.Builders.Seeders
{
    using Craftsman.Builders.Dtos;
    using Craftsman.Enums;
    using Craftsman.Exceptions;
    using Craftsman.Helpers;
    using Craftsman.Models;
    using LibGit2Sharp;
    using System;
    using System.Collections.Generic;
    using System.IO;
    using System.Linq;
    using System.Reflection.Emit;
    using System.Text;
    using static Helpers.ConsoleWriter;

    public class RoleSeedBuilder
    {
        public static void SeedRoles(string solutionDirectory, List<ApplicationUser> inMemoryUsers)
        {
            try
            {
                foreach (var user in inMemoryUsers)
                {
                    var classPath = ClassPathHelper.IdentitySeederClassPath(solutionDirectory, $"RoleSeeder.cs");

                    if (!Directory.Exists(classPath.ClassDirectory))
                        Directory.CreateDirectory(classPath.ClassDirectory);

                    if (File.Exists(classPath.FullClassPath))
                        throw new FileAlreadyExistsException(classPath.FullClassPath);

                    using (FileStream fs = File.Create(classPath.FullClassPath))
                    {
                        var data = SeedRoles(classPath, user.AuthorizedRoles);
                        fs.Write(Encoding.UTF8.GetBytes(data));
                    }

                    GlobalSingleton.AddCreatedFile(classPath.FullClassPath.Replace($"{solutionDirectory}{Path.DirectorySeparatorChar}", ""));
                }

                //Confirm all seeder registrations done in startup, if not, do here?
            }
            catch (FileAlreadyExistsException e)
            {
                WriteError(e.Message);
                throw;
            }
            catch (Exception e)
            {
                WriteError($"An unhandled exception occured when running the API command.\nThe error details are: \n{e.Message}");
                throw;
            }
        }

        private static string SeedRoles(ClassPath classPath, List<string> roles)
        {
            var roleSeeds = "";

            foreach (var role in roles)
            {
                var newLine = role == roles.LastOrDefault() ? "" : $"{Environment.NewLine}";
                roleSeeds += @$"            await roleManager.CreateAsync(new IdentityRole(Role.SuperAdmin.ToString()));{newLine}";
            }
            
            return $@"namespace {classPath.ClassNamespace}
{{
    using Domain.Enums;
    using Microsoft.AspNetCore.Identity;
    using System.Threading.Tasks;
    
    public static class RoleSeeder
    {{
        public static async Task SeedDemoRolesAsync(RoleManager<IdentityRole> roleManager)
        {{
            //Seed Roles
            {roleSeeds}
        }}
    }}
}}";
        }
    }
}
