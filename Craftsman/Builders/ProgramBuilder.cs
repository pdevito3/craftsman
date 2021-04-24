namespace Craftsman.Builders
{
    using Craftsman.Exceptions;
    using Craftsman.Helpers;
    using System;
    using System.IO.Abstractions;
    using System.Text;
    using static Helpers.ConsoleWriter;

    public class ProgramBuilder
    {
        public static void CreateWebApiProgram(string solutionDirectory, string projectBaseName, IFileSystem fileSystem)
        {
            var classPath = ClassPathHelper.WebApiProjectRootClassPath(solutionDirectory, $"Program.cs", projectBaseName);

            if (!fileSystem.Directory.Exists(classPath.ClassDirectory))
                fileSystem.Directory.CreateDirectory(classPath.ClassDirectory);

            if (fileSystem.File.Exists(classPath.FullClassPath))
                throw new FileAlreadyExistsException(classPath.FullClassPath);

            using (var fs = fileSystem.File.Create(classPath.FullClassPath))
            {
                var data = "";
                data = GetProgramText(classPath.ClassNamespace);
                fs.Write(Encoding.UTF8.GetBytes(data));
            }
        }

        public static string GetProgramText(string classNamespace)
        {
            return @$"namespace {classNamespace}
{{
    using Autofac.Extensions.DependencyInjection;
    using Microsoft.AspNetCore.Hosting;
    using Microsoft.Extensions.Configuration;
    using Microsoft.Extensions.DependencyInjection;
    using Microsoft.Extensions.Hosting;
    using Serilog;
    using System;
    using System.IO;
    using System.Reflection;
    using System.Threading.Tasks;

    public class Program
    {{
        public async static Task Main(string[] args)
        {{
            var host = CreateHostBuilder(args).Build();

            using var scope = host.Services.CreateScope();

            //Read configuration from appSettings
            var services = scope.ServiceProvider;
            var hostEnvironment = services.GetService<IWebHostEnvironment>();
            var config = new ConfigurationBuilder()
                .AddJsonFile(""appsettings.json"")
                .AddJsonFile($""appsettings.{{hostEnvironment.EnvironmentName}}.json"", true)
                .Build();

            //Initialize Logger
            Log.Logger = new LoggerConfiguration()
                .ReadFrom.Configuration(config)
                .CreateLogger();

            try
            {{
                Log.Information(""Starting application"");
                await host.RunAsync();
            }}
            catch (Exception e)
            {{
                Log.Error(e, ""The application failed to start correctly"");
                throw;
            }}
            finally
            {{
                Log.Information(""Shutting down application"");
                Log.CloseAndFlush();
            }}
        }}

        public static IHostBuilder CreateHostBuilder(string[] args) =>
            Host.CreateDefaultBuilder(args)
                .UseSerilog()
                .UseServiceProviderFactory(new AutofacServiceProviderFactory())
                .ConfigureWebHostDefaults(webBuilder =>
                {{
                    webBuilder.UseStartup(typeof(Startup).GetTypeInfo().Assembly.FullName)
                    .UseContentRoot(Directory.GetCurrentDirectory())
                    .UseKestrel();
                }});
    }}
}}";
        }
    }
}