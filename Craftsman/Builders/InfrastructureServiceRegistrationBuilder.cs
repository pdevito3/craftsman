namespace Craftsman.Builders;

using Helpers;
using Services;

public class InfrastructureServiceRegistrationBuilder
{
    private readonly ICraftsmanUtilities _utilities;

    public InfrastructureServiceRegistrationBuilder(ICraftsmanUtilities utilities)
    {
        _utilities = utilities;
    }
    public void CreateInfrastructureServiceExtension(string srcDirectory, string projectBaseName)
    {
        var classPath = ClassPathHelper.WebApiServiceExtensionsClassPath(srcDirectory, $"{FileNames.GetInfraRegistrationName()}.cs", projectBaseName);
        var fileText = GetServiceRegistrationText(srcDirectory, projectBaseName, classPath.ClassNamespace);
        _utilities.CreateFile(classPath, fileText);
    }

    public static string GetServiceRegistrationText(string srcDirectory, string projectBaseName, string classNamespace)
    {
        var dbContextClassPath = ClassPathHelper.DbContextClassPath(srcDirectory, "", projectBaseName);
        var utilsClassPath = ClassPathHelper.WebApiResourcesClassPath(srcDirectory, "", projectBaseName);
        var envServiceClassPath = ClassPathHelper.WebApiServicesClassPath(srcDirectory, "", projectBaseName);
        return @$"namespace {classNamespace};

using {dbContextClassPath.ClassNamespace};
using {utilsClassPath.ClassNamespace};
using {envServiceClassPath.ClassNamespace};
using Microsoft.EntityFrameworkCore;

public static class ServiceRegistration
{{
    public static void AddInfrastructure(this IServiceCollection services, IWebHostEnvironment env)
    {{
        // DbContext -- Do Not Delete

        // Auth -- Do Not Delete
    }}
}}";
    }
}
