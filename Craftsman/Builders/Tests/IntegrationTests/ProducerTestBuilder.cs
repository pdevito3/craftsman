namespace Craftsman.Builders.Tests.IntegrationTests
{
    using Craftsman.Enums;
    using Craftsman.Exceptions;
    using Craftsman.Helpers;
    using Craftsman.Models;
    using System.IO;
    using System.IO.Abstractions;
    using System.Text;

    public class ProducerTestBuilder
    {
        public static void CreateTests(string testDirectory, Producer producer, string projectBaseName, IFileSystem fileSystem)
        {
            var classPath = ClassPathHelper.FeatureTestClassPath(testDirectory, $"{producer.ProducerName}Tests.cs", "EventHandlers", projectBaseName);
            var fileText = WriteTestFileText(testDirectory, classPath, producer, projectBaseName);
            Utilities.CreateFile(classPath, fileText, fileSystem);
        }

        private static string WriteTestFileText(string solutionDirectory, ClassPath classPath, Producer producer, string projectBaseName)
        {
            var testFixtureName = Utilities.GetIntegrationTestFixtureName();
            var testUtilClassPath = ClassPathHelper.IntegrationTestUtilitiesClassPath(solutionDirectory, projectBaseName, "");
            var producerClassPath = ClassPathHelper.ProducerFeaturesClassPath(solutionDirectory, "", projectBaseName);
            
            return @$"namespace {classPath.ClassNamespace};

using FluentAssertions;
using NUnit.Framework;
using System.Threading.Tasks;
using MassTransit;
using MassTransit.Testing;
using Messages;
using Microsoft.Extensions.DependencyInjection;
using Moq;
using {producerClassPath.ClassNamespace};
using {testUtilClassPath.ClassNamespace};
using static {testFixtureName};

public class {producer.ProducerName}Tests : TestBase
{{
    {ProducerTest(producer)}
}}";
        }

        private static string ProducerTest(Producer producer)
        {
            var lowerProducerName = producer.ProducerName.ToLower();
            var messageName = producer.MessageName;

            return $@"[Test]
    public async Task {lowerProducerName}_can_produce_{producer.MessageName}_message()
    {{
        // Arrange
        var command = new {producer.ProducerName}.{producer.ProducerName}Command();

        // Act
        await SendAsync(command);

        // Assert
        (await IsFaultyPublished<{messageName}>()).Should().BeFalse();
        (await IsPublished<{messageName}>()).Should().BeTrue();
    }}";
        }
    }
}