namespace Craftsman.Builders;

using Domain;
using Domain.Enums;
using Helpers;
using Services;

public class ProducerRegistrationBuilder
{
    private readonly ICraftsmanUtilities _utilities;

    public ProducerRegistrationBuilder(ICraftsmanUtilities utilities)
    {
        _utilities = utilities;
    }

    public void CreateProducerRegistration(string solutionDirectory, string srcDirectory, Producer producer, string projectBaseName)
    {
        var className = $@"{producer.EndpointRegistrationMethodName}Registration";
        var classPath = ClassPathHelper.WebApiProducersServiceExtensionsClassPath(srcDirectory, $"{className}.cs", projectBaseName);
        var fileText = "";

        if (ExchangeTypeEnum.FromName(producer.ExchangeType) == ExchangeTypeEnum.Direct
            || ExchangeTypeEnum.FromName(producer.ExchangeType) == ExchangeTypeEnum.Topic)
            fileText = GetDirectOrTopicProducerRegistration(solutionDirectory, classPath.ClassNamespace, className, producer);
        else
            fileText = GetFanoutProducerRegistration(solutionDirectory, classPath.ClassNamespace, className, producer);

        _utilities.CreateFile(classPath, fileText);
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
