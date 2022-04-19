namespace NewCraftsman.Builders
{
    using System.IO;

    public class InfrastructureServiceRegistrationModifier
    {
        public static void InitializeAuthServices(string srcDirectory, string projectBaseName)
        {
            var classPath = ClassPathHelper.WebApiServiceExtensionsClassPath(srcDirectory, $"{Utilities.GetInfraRegistrationName()}.cs", projectBaseName);
            var servicesClassPath = ClassPathHelper.WebApiServicesClassPath(srcDirectory, "", projectBaseName);

            if (!Directory.Exists(classPath.ClassDirectory))
                Directory.CreateDirectory(classPath.ClassDirectory);

            if (!File.Exists(classPath.FullClassPath))
                throw new FileNotFoundException($"The `{classPath.FullClassPath}` file could not be found.");

            var authUsings = $@"
using Microsoft.AspNetCore.Authentication.JwtBearer;
using HeimGuard;
using {servicesClassPath.ClassNamespace};";
            var authServices = $@"
        if (!env.IsEnvironment(LocalConfig.FunctionalTestingEnvName))
        {{
            services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
                .AddJwtBearer(options =>
                {{
                    options.Authority = Environment.GetEnvironmentVariable(""AUTH_AUTHORITY"");
                    options.Audience = Environment.GetEnvironmentVariable(""AUTH_AUDIENCE"");
                }});
        }}

        services.AddAuthorization(options =>
        {{
        }});

        services.AddHeimGuard<UserPolicyHandler>()
            .MapAuthorizationPolicies()
            .AutomaticallyCheckPermissions();";

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
    }
}

