namespace Craftsman.Builders
{
    using Craftsman.Enums;
    using Craftsman.Exceptions;
    using Craftsman.Helpers;
    using Craftsman.Models;
    using System;
    using System.IO;
    using System.Text;

    public class ConsumerBuilder
    {
        public static void CreateConsumerFeature(string solutionDirectory, Consumer consumer, string projectBaseName)
        {
            var classPath = ClassPathHelper.ConsumerFeaturesClassPath(solutionDirectory, $"{consumer.ConsumerName}.cs", projectBaseName);

            if (!Directory.Exists(classPath.ClassDirectory))
                Directory.CreateDirectory(classPath.ClassDirectory);

            if (File.Exists(classPath.FullClassPath))
                throw new FileAlreadyExistsException(classPath.FullClassPath);

            using FileStream fs = File.Create(classPath.FullClassPath);
            var data = GetDirectOrTopicConsumerRegistration(classPath.ClassNamespace, consumer);

            fs.Write(Encoding.UTF8.GetBytes(data));
        }

        public static string GetDirectOrTopicConsumerRegistration(string classNamespace, Consumer consumer)
        {
            return @$"namespace {classNamespace}
{{
    using MassTransit;
    using Messages;
    using System.Threading.Tasks;

    public class {consumer.ConsumerName} : IConsumer<{consumer.MessageName}>
    {{
        public Task Consume(ConsumeContext<{consumer.MessageName}> context)
        {{
            // do work here

            return Task.CompletedTask;
        }}
    }}
}}";
        }
    }
}