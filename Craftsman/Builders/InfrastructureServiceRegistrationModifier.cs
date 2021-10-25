namespace Craftsman.Builders
{
    using Craftsman.Helpers;
    using Craftsman.Models;
    using System;
    using System.Collections.Generic;
    using System.IO;

    public class InfrastructureServiceRegistrationModifier
    {
        public static void InitializeAuthServices(string srcDirectory, string projectBaseName, List<Policy> policies)
        {
            var classPath = ClassPathHelper.WebApiServiceExtensionsClassPath(srcDirectory, $"{Utilities.GetInfraRegistrationName()}.cs", projectBaseName);

            if (!Directory.Exists(classPath.ClassDirectory))
                Directory.CreateDirectory(classPath.ClassDirectory);

            if (!File.Exists(classPath.FullClassPath))
                throw new FileNotFoundException($"The `{classPath.FullClassPath}` file could not be found.");

            var authUsings = $@"
    using Microsoft.AspNetCore.Authentication.JwtBearer;";
            var authServices = GetAuthServicesText();

            var tempPath = $"{classPath.FullClassPath}temp";
            using (var input = File.OpenText(classPath.FullClassPath))
            {
                using (var output = new StreamWriter(tempPath))
                {
                    string line;
                    bool usingsAdded = false;
                    while (null != (line = input.ReadLine()))
                    {
                        var newText = $"{line}";
                        if (line.Contains($"using") && !usingsAdded)
                        {
                            newText += authUsings;
                            usingsAdded = true;
                        }
                        else if (line.Contains($"// Auth -- Do Not Delete"))
                        {
                            newText += authServices;
                        }

                        output.WriteLine(newText);
                    }
                }
            }

            // delete the old file and set the name of the new one to the original name
            File.Delete(classPath.FullClassPath);
            File.Move(tempPath, classPath.FullClassPath);
        }

        public static void AddPolicies(string srcDirectory, List<Policy> policies, string projectBaseName)
        {
            var classPath = ClassPathHelper.WebApiServiceExtensionsClassPath(srcDirectory, $"{Utilities.GetInfraRegistrationName()}.cs", projectBaseName);

            if (!Directory.Exists(classPath.ClassDirectory))
                Directory.CreateDirectory(classPath.ClassDirectory);

            if (!File.Exists(classPath.FullClassPath))
                throw new FileNotFoundException($"The `{classPath.FullClassPath}` file could not be found.");

            var policiesString = "";
            var nonExistantPolicies = Utilities.GetPoliciesThatDoNotExist(policies, classPath.FullClassPath);
            foreach (var policy in nonExistantPolicies)
            {
                policiesString += $@"{Environment.NewLine}{Utilities.PolicyInfraStringBuilder(policy)}";
            }

            var tempPath = $"{classPath.FullClassPath}temp";
            using (var input = File.OpenText(classPath.FullClassPath))
            {
                using (var output = new StreamWriter(tempPath))
                {
                    string line;
                    bool updateNextLine = false;
                    while (null != (line = input.ReadLine()))
                    {
                        var newText = $"{line}";
                        if (line.Contains($"AddAuthorization"))
                        {
                            updateNextLine = true;
                        }
                        else if (updateNextLine)
                        {
                            newText += policiesString;
                            updateNextLine = false;
                        }

                        output.WriteLine(newText);
                    }
                }
            }

            // delete the old file and set the name of the new one to the original name
            File.Delete(classPath.FullClassPath);
            File.Move(tempPath, classPath.FullClassPath);
        }

        private static string GetAuthServicesText()
        {
            return $@"
            if (!env.IsEnvironment(LocalConfig.FunctionalTestingEnvName))
            {{
                services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
                    .AddJwtBearer(options =>
                    {{
                        options.Authority = configuration[""JwtSettings:Authority""];
                        options.Audience = configuration[""JwtSettings:Audience""];
                    }});
            }}

            services.AddAuthorization(options =>
            {{
            }});";
        }
    }
}

