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
            try
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

        public static void CreateGatewayProgram(string solutionDirectory, string gatewayProjectName, IFileSystem fileSystem)
        {
            try
            {
                var classPath = ClassPathHelper.GatewayProjectRootClassPath(solutionDirectory, $"Program.cs", gatewayProjectName);

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

        public static string GetProgramText(string classNamespace)
        {
            return @$"namespace {classNamespace}
{{
    using Autofac.Extensions.DependencyInjection;
    using Microsoft.AspNetCore.Hosting;
    using Microsoft.Extensions.Configuration;
    using Microsoft.Extensions.Hosting;
    using Serilog;
    using System;
    using System.IO;
    using System.Reflection;

    public class Program
    {{
        public static void Main(string[] args)
        {{
            var myEnv = Environment.GetEnvironmentVariable(""ASPNETCORE_ENVIRONMENT"");
            var appSettings = myEnv == null ? $""appsettings.json"" : $""appsettings.{{myEnv}}.json"";

            //Read Configuration from appSettings
            var config = new ConfigurationBuilder()
                .AddJsonFile(appSettings)
                .Build();

            //Initialize Logger
            Log.Logger = new LoggerConfiguration()
                .ReadFrom.Configuration(config)
                .CreateLogger();

            try
            {{
                Log.Information(""Starting application"");
                CreateHostBuilder(args)
                    .Build()
                    .Run();
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
