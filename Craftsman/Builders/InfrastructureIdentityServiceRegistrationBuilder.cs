namespace Craftsman.Builders
{
    using Craftsman.Exceptions;
    using Craftsman.Helpers;
    using Craftsman.Models;
    using System;
    using System.Collections.Generic;
    using System.IO.Abstractions;
    using System.Text;
    using static Helpers.ConsoleWriter;

    public class InfrastructureIdentityServiceRegistrationBuilder
    {
        public static void CreateInfrastructureIdentityServiceExtension(string solutionDirectory, List<Policy> policies, IFileSystem fileSystem)
        {
            try
            {
                var classPath = ClassPathHelper.InfrastructureIdentityProjectRootClassPath(solutionDirectory, $"ServiceRegistration.cs");

                if (!fileSystem.Directory.Exists(classPath.ClassDirectory))
                    fileSystem.Directory.CreateDirectory(classPath.ClassDirectory);

                if (fileSystem.File.Exists(classPath.FullClassPath))
                    throw new FileAlreadyExistsException(classPath.FullClassPath);

                using (var fs = fileSystem.File.Create(classPath.FullClassPath))
                {
                    var data = "";
                    data = GetServiceRegistrationText(classPath.ClassNamespace, policies);
                    fs.Write(Encoding.UTF8.GetBytes(data));
                }

                GlobalSingleton.AddCreatedFile(classPath.FullClassPath.Replace($"{solutionDirectory}{fileSystem.Path.DirectorySeparatorChar}", ""));
            }
            catch (FileAlreadyExistsException e)
            {
                WriteError(e.Message);
                throw;
            }
            catch (Exception e)
            {
                WriteError($"An unhandled exception occurred when running the API command.\nThe error details are: \n{e.Message}");
                throw;
            }
        }

        public static string GetServiceRegistrationText(string classNamespace, List<Policy> policies)
        {
            var policiesString = "";
            foreach (var policy in policies)
            {
                policiesString += $@"{Environment.NewLine}{Utilities.PolicyStringBuilder(policy)}";
            }

            return @$"namespace {classNamespace}
{{
    using Microsoft.AspNetCore.Authentication.JwtBearer;
    using Microsoft.Extensions.Configuration;
    using Microsoft.Extensions.DependencyInjection;

    public static class ServiceRegistration
    {{
        public static void AddIdentityInfrastructure(this IServiceCollection services, IConfiguration configuration)
        {{
            services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
                .AddJwtBearer(options =>
                {{
                    options.Authority = configuration[""JwtSettings:Authority""];
                    options.Audience = configuration[""JwtSettings:Audience""];
                }});
            
            services.AddAuthorization(options =>
            {{{policiesString}
            }});
        }}
    }}
}}
";
        }
    }
}
