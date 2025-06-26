namespace Craftsman.Builders;

using Domain;
using Domain.Enums;
using Helpers;
using MediatR;
using Services;

public static class ConsumerRegistrationBuilder
{
    public sealed record ConsumerRegistrationBuilderCommand(string SolutionDirectory, Consumer Consumer, string ProjectBaseName) : IRequest;

    public class Handler(ICraftsmanUtilities utilities) : IRequestHandler<ConsumerRegistrationBuilderCommand>
    {
        public Task Handle(ConsumerRegistrationBuilderCommand request, CancellationToken cancellationToken)
        {
            var className = $@"{request.Consumer.EndpointRegistrationMethodName}Registration";
            var classPath = ClassPathHelper.WebApiConsumersServiceExtensionsClassPath(request.SolutionDirectory, $"{className}.cs", request.ProjectBaseName);
            var consumerFeatureClassPath = ClassPathHelper.ConsumerFeaturesClassPath(request.SolutionDirectory, $"{request.Consumer.ConsumerName}.cs", request.Consumer.DomainDirectory, request.ProjectBaseName);

            var quorumText = request.Consumer.IsQuorum ? $@"

            // a replicated queue to provide high availability and data safety. available in RMQ 3.8+
            re.SetQuorumQueue();" : "";

            var lazyText = request.Consumer.IsLazy ? $@"

            // enables a lazy queue for more stable cluster with better predictive performance.
            // Please note that you should disable lazy queues if you require really high performance, if the queues are always short, or if you have set a max-length policy.
            re.SetQueueArgument(""declare"", ""lazy"");" : "";

            var data = "";

            if (ExchangeTypeEnum.FromName(request.Consumer.ExchangeType) == ExchangeTypeEnum.Direct
                || ExchangeTypeEnum.FromName(request.Consumer.ExchangeType) == ExchangeTypeEnum.Topic)
                data = GetDirectOrTopicConsumerRegistration(classPath.ClassNamespace, className, request.Consumer, lazyText, quorumText, consumerFeatureClassPath.ClassNamespace);
            else
                data = GetFanoutConsumerRegistration(classPath.ClassNamespace, className, request.Consumer, lazyText, quorumText, consumerFeatureClassPath.ClassNamespace);

            utilities.CreateFile(classPath, data);
            return Task.CompletedTask;
        }

    public static string GetDirectOrTopicConsumerRegistration(string classNamespace, string className, Consumer consumer, string lazyText, string quorumText, string consumerFeatureUsing)
    {
        var exchangeType = ExchangeTypeEnum.FromName(consumer.ExchangeType) == ExchangeTypeEnum.Direct
            ? "ExchangeType.Direct"
            : "ExchangeType.Topic";

        return @$"namespace {classNamespace};

using MassTransit;
using MassTransit.RabbitMqTransport;
using RabbitMQ.Client;
using {consumerFeatureUsing};

public static class {className}
{{
    public static void {consumer.EndpointRegistrationMethodName}(this IRabbitMqBusFactoryConfigurator cfg, IBusRegistrationContext context)
    {{
        cfg.ReceiveEndpoint(""{consumer.QueueName}"", re =>
        {{
            // turns off default fanout settings
            re.ConfigureConsumeTopology = false;{quorumText}{lazyText}

            // the consumers that are subscribed to the endpoint
            re.ConfigureConsumer<{consumer.ConsumerName}>(context);

            // the binding of the intermediary exchange and the primary exchange
            re.Bind(""{consumer.ExchangeName}"", e =>
            {{
                e.RoutingKey = ""{consumer.RoutingKey}"";
                e.ExchangeType = {exchangeType};
            }});
        }});
    }}
}}";
    }

    public static string GetFanoutConsumerRegistration(string classNamespace, string className, Consumer consumer, string lazyText, string quorumText, string consumerFeatureUsing)
    {
        return @$"namespace {classNamespace};

using MassTransit;
using MassTransit.RabbitMqTransport;
using RabbitMQ.Client;
using {consumerFeatureUsing};

public static class {className}
{{
    public static void {consumer.EndpointRegistrationMethodName}(this IRabbitMqBusFactoryConfigurator cfg, IBusRegistrationContext context)
    {{
        cfg.ReceiveEndpoint(""{consumer.QueueName}"", re =>
        {{
            // turns off default fanout settings
            re.ConfigureConsumeTopology = false;{quorumText}{lazyText}

            // the consumers that are subscribed to the endpoint
            re.ConfigureConsumer<{consumer.ConsumerName}>(context);

            // the binding of the intermediary exchange and the primary exchange
            re.Bind(""{consumer.ExchangeName}"", e =>
            {{
                e.ExchangeType = ExchangeType.Fanout;
            }});
        }});
    }}
}}";
        }
    }
}
