namespace Craftsman.Builders.ExtensionBuilders;

using Helpers;
using Services;

public class MassTransitExtensionsBuilder
{
    private readonly ICraftsmanUtilities _utilities;

    public MassTransitExtensionsBuilder(ICraftsmanUtilities utilities)
    {
        _utilities = utilities;
    }

    public void CreateMassTransitServiceExtension(string solutionDirectory, string srcDirectory, string projectBaseName)
    {
        var classPath = ClassPathHelper.WebApiServiceExtensionsClassPath(srcDirectory, $"{FileNames.GetMassTransitRegistrationName()}.cs", projectBaseName);
        var fileText = GetMassTransitServiceExtensionText(classPath.ClassNamespace, solutionDirectory, srcDirectory, projectBaseName);
        _utilities.CreateFile(classPath, fileText);
    }

    public static string GetMassTransitServiceExtensionText(string classNamespace, string solutionDirectory, string srcDirectory, string projectBaseName)
    {
        var utilsClassPath = ClassPathHelper.WebApiResourcesClassPath(srcDirectory, "", projectBaseName);
        var envServiceClassPath = ClassPathHelper.WebApiServicesClassPath(srcDirectory, "", projectBaseName);
        var messagesClassPath = ClassPathHelper.MessagesClassPath(solutionDirectory, "");

        return @$"namespace {classNamespace};

using {utilsClassPath.ClassNamespace};
using {envServiceClassPath.ClassNamespace};
using {messagesClassPath.ClassNamespace};
using Configurations;
using MassTransit;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using System.Reflection;

public static class MassTransitServiceExtension
{{
    public static void AddMassTransitServices(this IServiceCollection services, IWebHostEnvironment env, IConfiguration configuration)
    {{
        var rmqOptions = configuration.GetRabbitMqOptions();

        if (!env.IsEnvironment(Consts.Testing.IntegrationTestingEnvName) 
            && !env.IsEnvironment(Consts.Testing.FunctionalTestingEnvName))
        {{
            services.AddMassTransit(mt =>
            {{
                mt.AddConsumers(Assembly.GetExecutingAssembly());
                mt.UsingRabbitMq((context, cfg) =>
                {{
                    cfg.Host(rmqOptions.Host, 
                        ushort.Parse(rmqOptions.Port), 
                        rmqOptions.VirtualHost, 
                        h =>
                        {{
                            h.Username(rmqOptions.Username);
                            h.Password(rmqOptions.Password);
                        }});

                    // Producers -- Do Not Delete This Comment

                    // Consumers -- Do Not Delete This Comment
                }});
            }});
            services.AddOptions<MassTransitHostOptions>();
        }}
    }}
}}";
    }
}