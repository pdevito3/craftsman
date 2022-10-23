namespace Craftsman.Builders;

using System.IO;
using System.IO.Abstractions;
using Services;

public class InfrastructureServiceRegistrationModifier
{
    private readonly IFileSystem _fileSystem;

    public InfrastructureServiceRegistrationModifier(IFileSystem fileSystem)
    {
        _fileSystem = fileSystem;
    }
    public void InitializeAuthServices(string srcDirectory, string projectBaseName)
    {
        var classPath = ClassPathHelper.WebApiServiceExtensionsClassPath(srcDirectory, $"{FileNames.GetInfraRegistrationName()}.cs", projectBaseName);
        var servicesClassPath = ClassPathHelper.WebApiServicesClassPath(srcDirectory, "", projectBaseName);

        if (!_fileSystem.Directory.Exists(classPath.ClassDirectory))
            _fileSystem.Directory.CreateDirectory(classPath.ClassDirectory);

        if (!_fileSystem.File.Exists(classPath.FullClassPath))
            throw new FileNotFoundException($"The `{classPath.FullClassPath}` file could not be found.");

        var authUsings = $@"
using Microsoft.AspNetCore.Authentication.JwtBearer;
using HeimGuard;
using {servicesClassPath.ClassNamespace};";
        var authServices = $@"
        if (!env.IsEnvironment(Consts.Testing.FunctionalTestingEnvName))
        {{
            services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
                .AddJwtBearer(options =>
                {{
                    options.Authority = Environment.GetEnvironmentVariable(""AUTH_AUTHORITY"");
                    options.Audience = Environment.GetEnvironmentVariable(""AUTH_AUDIENCE"");
                    options.RequireHttpsMetadata = !env.IsDevelopment();
                }});
        }}

        services.AddAuthorization(options =>
        {{
        }});

        services.AddHeimGuard<UserPolicyHandler>()
            .MapAuthorizationPolicies()
            .AutomaticallyCheckPermissions();";

        var tempPath = $"{classPath.FullClassPath}temp";
        using (var input = _fileSystem.File.OpenText(classPath.FullClassPath))
        {
            using var output = _fileSystem.File.CreateText(tempPath);
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
        _fileSystem.File.Delete(classPath.FullClassPath);
        _fileSystem.File.Move(tempPath, classPath.FullClassPath);
    }
}

