namespace Craftsman.Builders.Tests.Utilities;

using System;
using System.IO;
using System.IO.Abstractions;
using Services;

public class IntegrationTestFixtureModifier
{
    private readonly IFileSystem _fileSystem;

    public IntegrationTestFixtureModifier(IFileSystem fileSystem)
    {
        _fileSystem = fileSystem;
    }

    public void AddMassTransit(string testDirectory, string projectBaseName)
    {
        var classPath = ClassPathHelper.IntegrationTestProjectRootClassPath(testDirectory, "TestFixture.cs", projectBaseName);

        if (!_fileSystem.Directory.Exists(classPath.ClassDirectory))
            throw new DirectoryNotFoundException($"The `{classPath.ClassDirectory}` directory could not be found.");

        if (!_fileSystem.File.Exists(classPath.FullClassPath))
            throw new FileNotFoundException($"The `{classPath.FullClassPath}` file could not be found.");

        var tempPath = $"{classPath.FullClassPath}temp";
        using (var input = _fileSystem.File.OpenText(classPath.FullClassPath))
        {
            using var output = _fileSystem.File.CreateText(tempPath);
            string line;
            while (null != (line = input.ReadLine()))
            {
                var newText = $"{line}";
                if (line.Contains($"MassTransit Harness Setup"))
                {
                    newText += $@"
        services.AddMassTransitInMemoryTestHarness(cfg =>
        {{
            cfg.AddMassTransitTestHarness(harness => 
            {{
                // Consumer Registration -- Do Not Delete Comment
            }});
        }});";
                }
                else if (line.Contains($"MassTransit Start Setup"))
                {
                    newText += $@"
        _harness = _provider.GetRequiredService<InMemoryTestHarness>();
        await _harness.Start();";
                }
                else if (line.Contains($"// MassTransit Teardown -- Do Not Delete Comment"))
                {
                    newText += $@"
        await _harness.Stop();";
                }
                else if (line.Contains($"// MassTransit Methods -- Do Not Delete Comment"))
                {
                    newText += $@"
    /// <summary>
    /// Publishes a message to the bus, and waits for the specified response.
    /// </summary>
    /// <param name=""message"">The message that should be published.</param>
    /// <typeparam name=""TMessage"">The message that should be published.</typeparam>
    public static async Task PublishMessage<TMessage>(object message)
        where TMessage : class
    {{
        await _harness.Bus.Publish<TMessage>(message);
    }}
    
    /// <summary>
    /// Confirm that a message has been published for this harness.
    /// </summary>
    /// <typeparam name=""TMessage"">The message that should be published.</typeparam>
    /// <returns>A boolean of true if a message of the given type has been published.</returns>
    public static async Task<bool> IsPublished<TMessage>()
        where TMessage : class
    {{
        return await _harness.Published.Any<TMessage>();
    }}
    
    /// <summary>
    /// Confirm that a message has been consumed for this harness.
    /// </summary>
    /// <typeparam name=""TMessage"">The message that should be consumed.</typeparam>
    /// <returns>A boolean of true if a message of the given type has been consumed.</returns>
    public static async Task<bool> IsConsumed<TMessage>()
        where TMessage : class
    {{
        return await _harness.Consumed.Any<TMessage>();
    }}
    
    /// <summary>
    /// The desired consumer consumed the message.
    /// </summary>
    /// <typeparam name=""TMessage"">The message that should be consumed.</typeparam>
    /// <typeparam name=""TConsumedBy"">The consumer of the message.</typeparam>
    /// <returns>A boolean of true if a message of the given type has been consumed by the given consumer.</returns>
    public static async Task<bool> IsConsumed<TMessage, TConsumedBy>()
        where TMessage : class
        where TConsumedBy : class, IConsumer
    {{
        var consumerHarness = _provider.GetRequiredService<IConsumerTestHarness<TConsumedBy>>();
        return await consumerHarness.Consumed.Any<TMessage>();
    }}";
                }
                else if (line.Contains($"static ServiceProvider _provider;"))
                {
                    newText += $@"
    private static InMemoryTestHarness _harness;";
                }

                output.WriteLine(newText);
            }
        }

        // delete the old file and set the name of the new one to the original name
        _fileSystem.File.Delete(classPath.FullClassPath);
        _fileSystem.File.Move(tempPath, classPath.FullClassPath);
    }

    public void AddMasstransitConsumer(string testDirectory, string consumerName, string domainDirectory, string projectBaseName, string srcDirectory)
    {
        var classPath = ClassPathHelper.IntegrationTestProjectRootClassPath(testDirectory, "TestFixture.cs", projectBaseName);

        if (!_fileSystem.Directory.Exists(classPath.ClassDirectory))
            throw new DirectoryNotFoundException($"The `{classPath.ClassDirectory}` directory could not be found.");

        if (!_fileSystem.File.Exists(classPath.FullClassPath))
            throw new FileNotFoundException($"The `{classPath.FullClassPath}` file could not be found.");

        var consumerClassPath = ClassPathHelper.ConsumerFeaturesClassPath(srcDirectory, $"", domainDirectory, projectBaseName);

        var tempPath = $"{classPath.FullClassPath}temp";
        var hasUsingForConsumerNamespace = false;
        using (var input = _fileSystem.File.OpenText(classPath.FullClassPath))
        {
            using var output = _fileSystem.File.CreateText(tempPath);
            string line;
            while (null != (line = input.ReadLine()))
            {
                var newText = $"{line}";
                if (line.Contains($"// Consumer Registration -- Do Not Delete Comment"))
                {
                    newText += $@"

                harness.AddConsumer<{consumerName}>();";
                }
                if (line.Contains(consumerClassPath.ClassNamespace))
                    hasUsingForConsumerNamespace = true;

                output.WriteLine(newText);
            }
        }

        // delete the old file and set the name of the new one to the original name
        _fileSystem.File.Delete(classPath.FullClassPath);
        _fileSystem.File.Move(tempPath, classPath.FullClassPath);

        if (!hasUsingForConsumerNamespace)
        {
            using (var input = _fileSystem.File.OpenText(classPath.FullClassPath))
            {
                using var output = _fileSystem.File.CreateText(tempPath);
                string line;
                while (null != (line = input.ReadLine()))
                {
                    var newText = $"{line}";
                    if (line.Contains($"using MassTransit;"))
                        newText += @$"{Environment.NewLine}using {consumerClassPath.ClassNamespace};";

                    output.WriteLine(newText);
                }
            }

            // delete the old file and set the name of the new one to the original name
            _fileSystem.File.Delete(classPath.FullClassPath);
            _fileSystem.File.Move(tempPath, classPath.FullClassPath);
        }
    }
}
