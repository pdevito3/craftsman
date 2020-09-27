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

    public class IdentityServicesModifier
    {
        public static void SetIdentityOptions(string solutionDirectory, AuthSetup setup)
        {
            var classPath = ClassPathHelper.IdentityProjectPath(solutionDirectory, $"ServiceExtensions.cs");

            if (!Directory.Exists(classPath.ClassDirectory))
                throw new DirectoryNotFoundException($"The `{classPath.ClassDirectory}` directory could not be found.");

            if (!File.Exists(classPath.FullClassPath))
                throw new FileNotFoundException($"The `{classPath.FullClassPath}` file could not be found.");

            var tempPath = $"{classPath.FullClassPath}temp";
            using (var input = File.OpenText(classPath.FullClassPath))
            {
                using (var output = new StreamWriter(tempPath))
                {
                    string line;
                    while (null != (line = input.ReadLine()))
                    {
                        var newText = $"{line}";
                        if (line.Contains($"options.User.RequireUniqueEmail"))
                            newText = @$"                options.User.RequireUniqueEmail = {setup.IdentityRequirements.RequireUniqueEmail.ToString().ToLower()};";
                        else if (line.Contains($"options.Password.RequiredLength"))
                            newText = @$"                options.Password.RequiredLength = {setup.IdentityRequirements.RequiredLength};";
                        else if (line.Contains($"options.Password.RequireDigit"))
                            newText = @$"                options.Password.RequireDigit = {setup.IdentityRequirements.RequireDigit.ToString().ToLower()};";
                        else if (line.Contains($"options.Password.RequireLowercase"))
                            newText = @$"                options.Password.RequireLowercase = {setup.IdentityRequirements.RequireLowercase.ToString().ToLower()};";
                        else if (line.Contains($"options.Password.RequireUppercase"))
                            newText = @$"                options.Password.RequireUppercase = {setup.IdentityRequirements.RequireUppercase.ToString().ToLower()};";
                        else if (line.Contains($"options.Password.RequireNonAlphanumeric"))
                            newText = @$"                options.Password.RequireNonAlphanumeric = {setup.IdentityRequirements.RequireNonAlphanumeric.ToString().ToLower()};";

                        output.WriteLine(newText);
                    }
                }
            }

            // delete the old file and set the name of the new one to the original name
            File.Delete(classPath.FullClassPath);
            File.Move(tempPath, classPath.FullClassPath);

            GlobalSingleton.AddUpdatedFile(classPath.FullClassPath.Replace($"{solutionDirectory}{Path.DirectorySeparatorChar}", ""));
        }
    }
}
