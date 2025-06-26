namespace Craftsman.Builders;

using Domain;
using Domain.Enums;
using Helpers;
using MediatR;
using Services;

public static class ProducerRegistrationBuilder
{
    public sealed record ProducerRegistrationBuilderCommand(string SolutionDirectory, string SrcDirectory, Producer Producer, string ProjectBaseName) : IRequest;

    public class Handler(ICraftsmanUtilities utilities) : IRequestHandler<ProducerRegistrationBuilderCommand>
    {
        public Task Handle(ProducerRegistrationBuilderCommand request, CancellationToken cancellationToken)
        {
            var className = $@"{request.Producer.EndpointRegistrationMethodName}Registration";
            var classPath = ClassPathHelper.WebApiProducersServiceExtensionsClassPath(request.SrcDirectory, $"{className}.cs", request.ProjectBaseName);
            var fileText = "";

            if (ExchangeTypeEnum.FromName(request.Producer.ExchangeType) == ExchangeTypeEnum.Direct
                || ExchangeTypeEnum.FromName(request.Producer.ExchangeType) == ExchangeTypeEnum.Topic)
                fileText = GetDirectOrTopicProducerRegistration(request.SolutionDirectory, classPath.ClassNamespace, className, request.Producer);
            else
                fileText = GetFanoutProducerRegistration(request.SolutionDirectory, classPath.ClassNamespace, className, request.Producer);

            utilities.CreateFile(classPath, fileText);
            return Task.CompletedTask;
        }

        public static string GetDirectOrTopicProducerRegistration(string solutionDirectory, string classNamespace, string className, Producer producer)
    {
        var exchangeType = ExchangeTypeEnum.FromName(producer.ExchangeType) == ExchangeTypeEnum.Direct
            ? "ExchangeType.Direct"
            : "ExchangeType.Topic";
        var messagesClassPath = ClassPathHelper.MessagesClassPath(solutionDirectory, "");

        return @$"namespace {classNamespace};

using MassTransit;
using MassTransit.RabbitMqTransport;
using {messagesClassPath.ClassNamespace};
using RabbitMQ.Client;

public static class {className}
{{
    public static void {producer.EndpointRegistrationMethodName}(this IRabbitMqBusFactoryConfigurator cfg)
    {{
        cfg.Message<{producer.MessageName}>(e => e.SetEntityName(""{producer.ExchangeName}"")); // name of the primary exchange
        cfg.Publish<{producer.MessageName}>(e => e.ExchangeType = {exchangeType}); // primary exchange type

        // configuration for the exchange and routing key
        cfg.Send<{producer.MessageName}>(e =>
        {{
            // **Use the `UseRoutingKeyFormatter` to configure what to use for the routing key when sending a message of type `{producer.MessageName}`**
            /* Examples
            *
            * Direct example: uses the `ProductType` message property as a key
            * e.UseRoutingKeyFormatter(context => context.Message.ProductType.ToString());
            *
            * Topic example: uses the VIP Status and ClientType message properties to make a key.
            * e.UseRoutingKeyFormatter(context =>
            * {{
            *     var vipStatus = context.Message.IsVip ? ""vip"" : ""normal"";
            *     return $""{{vipStatus}}.{{context.Message.ClientType}}"";
            * }});
            */
        }});
    }}
}}";
        }

        public static string GetFanoutProducerRegistration(string solutionDirectory, string classNamespace, string className, Producer producer)
    {
        var messagesClassPath = ClassPathHelper.MessagesClassPath(solutionDirectory, "");

        return @$"namespace {classNamespace};

using MassTransit;
using MassTransit.RabbitMqTransport;
using {messagesClassPath.ClassNamespace};
using RabbitMQ.Client;

public static class {className}
{{
    public static void {producer.EndpointRegistrationMethodName}(this IRabbitMqBusFactoryConfigurator cfg)
    {{
        cfg.Message<{producer.MessageName}>(e => e.SetEntityName(""{producer.ExchangeName}"")); // name of the primary exchange
        cfg.Publish<{producer.MessageName}>(e => e.ExchangeType = ExchangeType.Fanout); // primary exchange type
    }}
}}";
        }
    }
}
