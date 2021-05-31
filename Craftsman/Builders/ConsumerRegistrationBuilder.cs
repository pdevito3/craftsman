namespace Craftsman.Builders
{
    using Craftsman.Enums;
    using Craftsman.Exceptions;
    using Craftsman.Helpers;
    using Craftsman.Models;
    using System;
    using System.IO;
    using System.Text;

    public class ConsumerRegistrationBuilder
    {
        public static void CreateConsumerRegistration(string solutionDirectory, Consumer consumer, string projectBaseName)
        {
            var className = $@"{consumer.EndpointRegistrationMethodName}Registration";
            var classPath = ClassPathHelper.WebApiConsumersServiceExtensionsClassPath(solutionDirectory, $"{className}.cs", projectBaseName);
            var consumerFeatureClassPath = ClassPathHelper.ConsumerFeaturesClassPath(solutionDirectory, $"{consumer.ConsumerName}.cs", projectBaseName);

            if (!Directory.Exists(classPath.ClassDirectory))
                Directory.CreateDirectory(classPath.ClassDirectory);

            if (File.Exists(classPath.FullClassPath))
                throw new FileAlreadyExistsException(classPath.FullClassPath);

            var quorumText = consumer.IsQuorum ? $@"

                // a replicated queue to provide high availability and data safety. available in RMQ 3.8+
                re.SetQuorumQueue();" : "";

            var lazyText = consumer.IsLazy ? $@"

                // enables a lazy queue for more stable cluster with better predictive performance.
                // Please note that you should disable lazy queues if you require really high performance, if the queues are always short, or if you have set a max-length policy.
                re.SetQueueArgument(""declare"", ""lazy"");" : "";
            //re.Lazy = true;" : "";

            using FileStream fs = File.Create(classPath.FullClassPath);
            var data = "";

            if (Enum.GetName(typeof(ExchangeType), ExchangeType.Direct) == consumer.ExchangeType
                || Enum.GetName(typeof(ExchangeType), ExchangeType.Topic) == consumer.ExchangeType)
                data = GetDirectOrTopicConsumerRegistration(classPath.ClassNamespace, className, consumer, lazyText, quorumText, consumerFeatureClassPath.ClassNamespace);
            else
                data = GetFanoutConsumerRegistration(classPath.ClassNamespace, className, consumer, lazyText, quorumText, consumerFeatureClassPath.ClassNamespace);

            fs.Write(Encoding.UTF8.GetBytes(data));
        }

        public static string GetDirectOrTopicConsumerRegistration(string classNamespace, string className, Consumer consumer, string lazyText, string quorumText, string consumerFeatureUsing)
        {
            var exchangeType = Enum.GetName(typeof(ExchangeType), ExchangeType.Direct) == consumer.ExchangeType ? "ExchangeType.Direct" : "ExchangeType.Topic";

            return @$"namespace {classNamespace}
{{
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
    }}
}}";
        }

        public static string GetFanoutConsumerRegistration(string classNamespace, string className, Consumer consumer, string lazyText, string quorumText, string consumerFeatureUsing)
        {
            return @$"namespace {classNamespace}
{{
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
    }}
}}";
        }
    }
}