namespace Craftsman.Builders.Tests.IntegrationTests
{
    using Craftsman.Enums;
    using Craftsman.Exceptions;
    using Craftsman.Helpers;
    using Craftsman.Models;
    using System.IO;
    using System.IO.Abstractions;
    using System.Text;

    public class ConsumerTestBuilder
    {
        public static void CreateTests(string srcDirectory, Consumer consumer, string projectBaseName, IFileSystem fileSystem)
        {
            var classPath = ClassPathHelper.FeatureTestClassPath(srcDirectory, $"{consumer.ConsumerName}Tests.cs", "EventHandlers", projectBaseName);
            var fileText = WriteTestFileText(srcDirectory, classPath, consumer, projectBaseName);
            Utilities.CreateFile(classPath, fileText, fileSystem);
        }

        private static string WriteTestFileText(string solutionDirectory, ClassPath classPath, Consumer consumer, string projectBaseName)
        {
            var testFixtureName = Utilities.GetIntegrationTestFixtureName();
            var testUtilClassPath = ClassPathHelper.IntegrationTestUtilitiesClassPath(solutionDirectory, projectBaseName, "");
            var consumerClassPath = ClassPathHelper.ConsumerFeaturesClassPath(solutionDirectory, "", projectBaseName);
            
            return @$"namespace {classPath.ClassNamespace};

using FluentAssertions;
using NUnit.Framework;
using System.Threading.Tasks;
using System;
using MassTransit;
using MassTransit.Testing;
using Messages;
using Microsoft.Extensions.DependencyInjection;
using Moq;
using {consumerClassPath.ClassNamespace};
using {testUtilClassPath.ClassNamespace};
using static {testFixtureName};

public class {consumer.ConsumerName}Tests : TestBase
{{
    {ConsumerTest(consumer)}
}}";
        }

        private static string ConsumerTest(Consumer consumer)
        {
            var lowerConsumerName = consumer.ConsumerName.ToLower();
            var messageName = consumer.MessageName;

            return $@"[Test]
    public async Task {lowerConsumerName}_can_consume_{consumer.MessageName}_message()
    {{
        // Arrange
        var message = new Mock<{messageName}>();

        // Act
        await PublishMessage<{messageName}>(message);

        // Assert
        // did the endpoint consume the message
        (await _harness.Consumed.Any<{messageName}>()).Should().Be(true);

        // ensure that no faults were published by the consumer
        (await _harness.Published.Any<Fault<{messageName}>>()).Should().Be(false);
        
        // the desired consumer consumed the message
        var consumerHarness = _provider.GetRequiredService<IConsumerTestHarness<{consumer.ConsumerName}>>();
        (await consumerHarness.Consumed.Any<{messageName}>()).Should().Be(true);
    }}";
        }
    }
}