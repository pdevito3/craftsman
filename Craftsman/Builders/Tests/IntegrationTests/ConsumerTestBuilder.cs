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
        public static void CreateTests(string solutionDirectory, string testDirectory, string srcDirectory, Consumer consumer, string projectBaseName, IFileSystem fileSystem)
        {
            var classPath = ClassPathHelper.FeatureTestClassPath(testDirectory, $"{consumer.ConsumerName}Tests.cs", "EventHandlers", projectBaseName);
            var fileText = WriteTestFileText(solutionDirectory, testDirectory, srcDirectory, classPath, consumer, projectBaseName);
            Utilities.CreateFile(classPath, fileText, fileSystem);
        }

        private static string WriteTestFileText(string solutionDirectory, string testDirectory, string srcDirectory, ClassPath classPath, Consumer consumer, string projectBaseName)
        {
            var testFixtureName = Utilities.GetIntegrationTestFixtureName();
            var testUtilClassPath = ClassPathHelper.IntegrationTestUtilitiesClassPath(testDirectory, projectBaseName, "");
            var consumerClassPath = ClassPathHelper.ConsumerFeaturesClassPath(srcDirectory, "", consumer.DomainDirectory, projectBaseName);
            
            var messagesClassPath = ClassPathHelper.MessagesClassPath(solutionDirectory, "");
            return @$"namespace {classPath.ClassNamespace};

using FluentAssertions;
using NUnit.Framework;
using System.Threading.Tasks;
using MassTransit;
using MassTransit.Testing;
using {messagesClassPath.ClassNamespace};
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
            var messageName = consumer.MessageName;

            return $@"[Test]
    public async Task can_consume_{consumer.MessageName}_message()
    {{
        // Arrange
        var message = new Mock<{messageName}>();

        // Act
        await PublishMessage<{messageName}>(message);

        // Assert
        (await IsConsumed<{messageName}>()).Should().Be(true);
        (await IsConsumed<{messageName}, {consumer.ConsumerName}>()).Should().Be(true);
    }}";
        }
    }
}