namespace NewCraftsman.Builders
{
    using System.IO;
    using System.IO.Abstractions;
    using System.Text;
    using Domain;
    using Domain.Enums;
    using Exceptions;
    using Helpers;
    using Services;

    public class ConsumerRegistrationBuilder
    {
        private readonly ICraftsmanUtilities _utilities;

        public ConsumerRegistrationBuilder(ICraftsmanUtilities utilities)
        {
            _utilities = utilities;
        }

        public void CreateConsumerRegistration(string solutionDirectory, Consumer consumer, string projectBaseName)
        {
            var className = $@"{consumer.EndpointRegistrationMethodName}Registration";
            var classPath = ClassPathHelper.WebApiConsumersServiceExtensionsClassPath(solutionDirectory, $"{className}.cs", projectBaseName);
            var consumerFeatureClassPath = ClassPathHelper.ConsumerFeaturesClassPath(solutionDirectory, $"{consumer.ConsumerName}.cs", consumer.DomainDirectory, projectBaseName);

            var quorumText = consumer.IsQuorum ? $@"

            // a replicated queue to provide high availability and data safety. available in RMQ 3.8+
            re.SetQuorumQueue();" : "";

            var lazyText = consumer.IsLazy ? $@"

            // enables a lazy queue for more stable cluster with better predictive performance.
            // Please note that you should disable lazy queues if you require really high performance, if the queues are always short, or if you have set a max-length policy.
            re.SetQueueArgument(""declare"", ""lazy"");" : "";
            //re.Lazy = true;" : "";

            var data = "";

            if (ExchangeTypeEnum.FromName(consumer.ExchangeType) == ExchangeTypeEnum.Direct
                || ExchangeTypeEnum.FromName(consumer.ExchangeType) == ExchangeTypeEnum.Topic)
                data = GetDirectOrTopicConsumerRegistration(classPath.ClassNamespace, className, consumer, lazyText, quorumText, consumerFeatureClassPath.ClassNamespace);
            else
                data = GetFanoutConsumerRegistration(classPath.ClassNamespace, className, consumer, lazyText, quorumText, consumerFeatureClassPath.ClassNamespace);

            _utilities.CreateFile(classPath, data);
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