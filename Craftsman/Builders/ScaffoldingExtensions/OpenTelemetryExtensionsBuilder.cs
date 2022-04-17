namespace Craftsman.Builders.ScaffoldingExtensions;

using System.IO.Abstractions;
using Enums;
using Helpers;

public class OpenTelemetryExtensionsBuilder
{
    public static void CreateOTelServiceExtension(string solutionDirectory, string projectBaseName, string dbProvider, IFileSystem fileSystem)
    {
        var classPath = ClassPathHelper.WebApiServiceExtensionsClassPath(solutionDirectory, $"OpenTelemetryServiceExtension.cs", projectBaseName);
        var fileText = GetApiVersioningServiceExtensionText(classPath.ClassNamespace, dbProvider);
        Utilities.CreateFile(classPath, fileText, fileSystem);
    }

    public static string GetApiVersioningServiceExtensionText(string classNamespace, string dbProvider)
    {
        var dbSource = Enum.GetName(typeof(DbProvider), DbProvider.Postgres) == dbProvider 
            ? "Npgsql" 
            : "Microsoft.EntityFrameworkCore.SqlServer";
        
        return @$"namespace {classNamespace};

using OpenTelemetry;
using OpenTelemetry.Resources;
using OpenTelemetry.Trace;

public static class OpenTelemetryServiceExtension
{{
    public static void OpenTelemetryRegistration(this IServiceCollection services, string serviceName)
    {{
        services.AddOpenTelemetryTracing(builder =>
        {{
            builder.SetResourceBuilder(ResourceBuilder.CreateDefault()
                    .AddService(serviceName)
                    .AddTelemetrySdk()
                    .AddEnvironmentVariableDetector())
                .AddSource(""MassTransit"")
                .AddSource(""{dbSource}"")
                // The following subscribes to activities from Activity Source
                // named ""MyCompany.MyProduct.MyLibrary"" only.
                // .AddSource(""MyCompany.MyProduct.MyLibrary"")
                .AddSqlClientInstrumentation(opt => opt.SetDbStatementForText = true)
                .AddAspNetCoreInstrumentation()
                .AddJaegerExporter(o =>
                {{
                    o.AgentHost = Environment.GetEnvironmentVariable(""JAEGER_HOST"");
                    o.AgentPort = 6831;
                    o.MaxPayloadSizeInBytes = 4096;
                    o.ExportProcessorType = ExportProcessorType.Batch;
                    o.BatchExportProcessorOptions = new BatchExportProcessorOptions<System.Diagnostics.Activity>
                    {{
                        MaxQueueSize = 2048,
                        ScheduledDelayMilliseconds = 5000,
                        ExporterTimeoutMilliseconds = 30000,
                        MaxExportBatchSize = 512,
                    }};
                }});
        }});
    }}
}}";
    }
}