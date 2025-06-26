namespace Craftsman.Builders.ExtensionBuilders;

using Domain;
using Helpers;
using MediatR;
using Services;

public static class OpenTelemetryExtensionsBuilder
{
    public sealed record OpenTelemetryExtensionsBuilderCommand(DbProvider DbProvider, int OtelAgentPort) : IRequest;

    public class Handler(ICraftsmanUtilities utilities, IScaffoldingDirectoryStore scaffoldingDirectoryStore)
        : IRequestHandler<OpenTelemetryExtensionsBuilderCommand>
    {
        public Task Handle(OpenTelemetryExtensionsBuilderCommand request, CancellationToken cancellationToken)
        {
            var classPath = ClassPathHelper.WebApiServiceExtensionsClassPath(scaffoldingDirectoryStore.SrcDirectory, $"OpenTelemetryServiceExtension.cs", scaffoldingDirectoryStore.ProjectBaseName);
            var fileText = GetOtelText(classPath.ClassNamespace, request.DbProvider, request.OtelAgentPort, scaffoldingDirectoryStore.SrcDirectory, scaffoldingDirectoryStore.ProjectBaseName);
            utilities.CreateFile(classPath, fileText);
            return Task.CompletedTask;
        }
    }

    public static string GetOtelText(string classNamespace, DbProvider dbProvider, int otelAgentPort, string srcDirectory, string projectBaseName)
    {
        var envServiceClassPath = ClassPathHelper.WebApiServicesClassPath(srcDirectory, "", projectBaseName);
        return @$"namespace {classNamespace};

using {envServiceClassPath.ClassNamespace};
using Resources;
using OpenTelemetry;
using OpenTelemetry.Metrics;
using OpenTelemetry.Resources;
using OpenTelemetry.Trace;

public static class OpenTelemetryServiceExtension
{{
    public static void OpenTelemetryRegistration(this WebApplicationBuilder builder, IConfiguration configuration, string serviceName)
    {{
        var resourceBuilder = ResourceBuilder.CreateDefault().AddService(serviceName)
            .AddTelemetrySdk()
            .AddEnvironmentVariableDetector();
        
        builder.Logging.AddOpenTelemetry(o =>
        {{
            // TODO: Setup an exporter here
            o.SetResourceBuilder(resourceBuilder);
        }});

        builder.Services.AddOpenTelemetry()
            .WithMetrics(metricsBuilder =>
                metricsBuilder.SetResourceBuilder(resourceBuilder)
                    .AddAspNetCoreInstrumentation()
                    .AddRuntimeInstrumentation()
                    .AddHttpClientInstrumentation()
                    .AddEventCountersInstrumentation(c =>
                    {{
                        // https://learn.microsoft.com/en-us/dotnet/core/diagnostics/available-counters
                        c.AddEventSources(
                            ""Microsoft.AspNetCore.Hosting"",
                            ""Microsoft-AspNetCore-Server-Kestrel"",
                            ""System.Net.Http"",
                            ""System.Net.Sockets"",
                            ""System.Net.NameResolution"",
                            ""System.Net.Security"");
                    }}))
            .WithTracing(tracerBuilder =>
                tracerBuilder.SetResourceBuilder(resourceBuilder)
                    .AddSource(""MassTransit"")
                    .AddSource(""{dbProvider.OTelSource()}"")
                    .AddSqlClientInstrumentation(opt => opt.SetDbStatementForText = true)
                    .AddAspNetCoreInstrumentation()
                    .AddHttpClientInstrumentation()
                    .AddEntityFrameworkCoreInstrumentation()
                    .AddJaegerExporter(o =>
                    {{
                        o.AgentHost = configuration.GetJaegerHostValue();
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
                    }}));
    }}
}}";
    }
}