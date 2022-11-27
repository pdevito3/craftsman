namespace Craftsman.Builders.ExtensionBuilders;

using Domain;
using Helpers;
using Services;

public class OpenTelemetryExtensionsBuilder
{
    private readonly ICraftsmanUtilities _utilities;

    public OpenTelemetryExtensionsBuilder(ICraftsmanUtilities utilities)
    {
        _utilities = utilities;
    }

    public void CreateOTelServiceExtension(string srcDirectory, string projectBaseName, DbProvider dbProvider, int otelAgentPort)
    {
        var classPath = ClassPathHelper.WebApiServiceExtensionsClassPath(srcDirectory, $"OpenTelemetryServiceExtension.cs", projectBaseName);
        var fileText = GetOtelText(classPath.ClassNamespace, dbProvider, otelAgentPort, srcDirectory, projectBaseName);
        _utilities.CreateFile(classPath, fileText);
    }

    public static string GetOtelText(string classNamespace, DbProvider dbProvider, int otelAgentPort, string srcDirectory, string projectBaseName)
    {
        var envServiceClassPath = ClassPathHelper.WebApiServicesClassPath(srcDirectory, "", projectBaseName);
        return @$"namespace {classNamespace};

using {envServiceClassPath.ClassNamespace};
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
                .AddSource(""{dbProvider.OTelSource()}"")
                // The following subscribes to activities from Activity Source
                // named ""MyCompany.MyProduct.MyLibrary"" only.
                // .AddSource(""MyCompany.MyProduct.MyLibrary"")
                .AddSqlClientInstrumentation(opt => opt.SetDbStatementForText = true)
                .AddAspNetCoreInstrumentation()
                .AddJaegerExporter(o =>
                {{
                    o.AgentHost = EnvironmentService.JaegerHost;
                    o.AgentPort = {otelAgentPort};
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