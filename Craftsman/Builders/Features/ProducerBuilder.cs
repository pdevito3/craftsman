namespace Craftsman.Builders.Features;

using System;
using Domain;
using Helpers;
using Services;
using MediatR;

public static class ProducerBuilder
{
    public sealed record Command(Producer Producer) : IRequest;

    public class Handler(
        ICraftsmanUtilities utilities,
        IScaffoldingDirectoryStore scaffoldingDirectoryStore)
        : IRequestHandler<Command>
    {
        public Task Handle(Command request, CancellationToken cancellationToken)
        {
            var classPath = ClassPathHelper.ProducerFeaturesClassPath(scaffoldingDirectoryStore.SrcDirectory, $"{request.Producer.ProducerName}.cs", request.Producer.DomainDirectory, scaffoldingDirectoryStore.ProjectBaseName);
            var fileText = GetProducerRegistration(classPath.ClassNamespace, request.Producer, scaffoldingDirectoryStore.SolutionDirectory, scaffoldingDirectoryStore.SrcDirectory, scaffoldingDirectoryStore.ProjectBaseName, utilities);
            utilities.CreateFile(classPath, fileText);
            return Task.CompletedTask;
        }
    }

    private static string GetProducerRegistration(string classNamespace, Producer producer, string solutionDirectory, string srcDirectory, string projectBaseName, ICraftsmanUtilities utilities)
    {
        var context = utilities.GetDbContext(srcDirectory, projectBaseName);
        var contextClassPath = ClassPathHelper.DbContextClassPath(srcDirectory, "", projectBaseName);
        var dbProp = producer.UsesDb ? @$"{context} dbContext, " : "";
        var contextUsing = producer.UsesDb ? $@"
using {contextClassPath.ClassNamespace};" : "";

        var messagesClassPath = ClassPathHelper.MessagesClassPath(solutionDirectory, "");
        
        var commandName = $"{producer.ProducerName}Command";

        return @$"namespace {classNamespace};

using {messagesClassPath.ClassNamespace};
using MassTransit;
using MediatR;
using Microsoft.EntityFrameworkCore;
using System.Threading;
using System.Threading.Tasks;{contextUsing}

public static class {producer.ProducerName}
{{
    public sealed record {commandName} : IRequest;

    public sealed class Handler({dbProp}IPublishEndpoint publishEndpoint) : IRequestHandler<{commandName}>
    {{

        public async Task Handle({commandName} request, CancellationToken cancellationToken)
        {{
            var message = new {FileNames.MessageClassName(producer.MessageName)}
            {{
                // map content to message here
            }};
            await publishEndpoint.Publish<{FileNames.MessageInterfaceName(producer.MessageName)}>(message, cancellationToken);
        }}
    }}
}}";
    }
}
