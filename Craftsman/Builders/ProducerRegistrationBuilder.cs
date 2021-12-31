namespace Craftsman.Builders
{
    using Craftsman.Enums;
    using Craftsman.Exceptions;
    using Craftsman.Helpers;
    using Craftsman.Models;
    using System;
    using System.IO;
    using System.Text;

    public class ProducerRegistrationBuilder
    {
        public static void CreateProducerRegistration(string solutionDirectory, Producer producer, string projectBaseName)
        {
            var className = $@"{producer.EndpointRegistrationMethodName}Registration";
            var classPath = ClassPathHelper.WebApiProducersServiceExtensionsClassPath(solutionDirectory, $"{className}.cs", projectBaseName);

            if (!Directory.Exists(classPath.ClassDirectory))
                Directory.CreateDirectory(classPath.ClassDirectory);

            if (File.Exists(classPath.FullClassPath))
                throw new FileAlreadyExistsException(classPath.FullClassPath);

            using FileStream fs = File.Create(classPath.FullClassPath);
            var data = "";

            if (ExchangeTypeEnum.FromName(producer.ExchangeType) == ExchangeTypeEnum.Direct
                || ExchangeTypeEnum.FromName(producer.ExchangeType) == ExchangeTypeEnum.Topic)
                data = GetDirectOrTopicProducerRegistration(classPath.ClassNamespace, className, producer);
            else
                data = GetFanoutProducerRegistration(classPath.ClassNamespace, className, producer);

            fs.Write(Encoding.UTF8.GetBytes(data));
        }

        public static string GetDirectOrTopicProducerRegistration(string classNamespace, string className, Producer producer)
        {
            var exchangeType = ExchangeTypeEnum.FromName(producer.ExchangeType) == ExchangeTypeEnum.Direct 
                ? "ExchangeType.Direct" 
                : "ExchangeType.Topic";

            return @$"namespace {classNamespace};

using MassTransit;
using MassTransit.RabbitMqTransport;
using Messages;
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

        public static string GetFanoutProducerRegistration(string classNamespace, string className, Producer producer)
        {
            return @$"namespace {classNamespace};

using MassTransit;
using MassTransit.RabbitMqTransport;
using Messages;
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