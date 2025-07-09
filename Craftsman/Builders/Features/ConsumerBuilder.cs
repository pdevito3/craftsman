namespace Craftsman.Builders.Features;

using System;
using Domain;
using Helpers;
using Services;
using MediatR;

public static class ConsumerBuilder
{
    public sealed record Command(Consumer Consumer) : IRequest;

    public class Handler(
        ICraftsmanUtilities utilities,
        IScaffoldingDirectoryStore scaffoldingDirectoryStore)
        : IRequestHandler<Command>
    {
        public Task Handle(Command request, CancellationToken cancellationToken)
        {
            var classPath = ClassPathHelper.ConsumerFeaturesClassPath(scaffoldingDirectoryStore.SrcDirectory, $"{request.Consumer.ConsumerName}.cs", request.Consumer.DomainDirectory, scaffoldingDirectoryStore.ProjectBaseName);
            var fileText = GetDirectOrTopicConsumerRegistration(classPath.ClassNamespace, request.Consumer, scaffoldingDirectoryStore.SolutionDirectory, scaffoldingDirectoryStore.SrcDirectory, scaffoldingDirectoryStore.ProjectBaseName, utilities);
            utilities.CreateFile(classPath, fileText);
            return Task.CompletedTask;
        }
    }

    private static string GetDirectOrTopicConsumerRegistration(string classNamespace, Consumer consumer, string solutionDirectory, string srcDirectory, string projectBaseName, ICraftsmanUtilities utilities)
    {
        var context = utilities.GetDbContext(srcDirectory, projectBaseName);
        var contextClassPath = ClassPathHelper.DbContextClassPath(srcDirectory, "", projectBaseName);
        var dbProp = consumer.UsesDb ? @$"{context} dbContext" : "";
        var contextUsing = consumer.UsesDb ? $@"
using {contextClassPath.ClassNamespace};" : "";

        var messagesClassPath = ClassPathHelper.MessagesClassPath(solutionDirectory, "");
        return @$"namespace {classNamespace};

using MassTransit;
using {messagesClassPath.ClassNamespace};
using System.Threading.Tasks;{contextUsing}

public sealed class {consumer.ConsumerName}({dbProp}) : IConsumer<{FileNames.MessageInterfaceName(consumer.MessageName)}>
{{
    public Task Consume(ConsumeContext<{FileNames.MessageInterfaceName(consumer.MessageName)}> context)
    {{
        // do work here

        return Task.CompletedTask;
    }}
}}";
    }
}
