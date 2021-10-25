namespace Craftsman.Builders.Tests.Utilities
{
    using Craftsman.Helpers;
    using Craftsman.Models;
    using System;
    using System.Collections.Generic;
    using System.IO;

    public class IntegrationTestFixtureModifier
    {
        public static void AddMassTransit(string testDirectory, string projectBaseName)
        {
            var classPath = ClassPathHelper.IntegrationTestProjectRootClassPath(testDirectory, "TestFixture.cs", projectBaseName);

            if (!Directory.Exists(classPath.ClassDirectory))
                throw new DirectoryNotFoundException($"The `{classPath.ClassDirectory}` directory could not be found.");

            if (!File.Exists(classPath.FullClassPath))
                throw new FileNotFoundException($"The `{classPath.FullClassPath}` file could not be found.");

            var consumerFeatureClassPath = ClassPathHelper.ConsumerFeaturesClassPath(testDirectory, $"", projectBaseName);

            var tempPath = $"{classPath.FullClassPath}temp";
            using (var input = File.OpenText(classPath.FullClassPath))
            {
                using var output = new StreamWriter(tempPath);
                string line;
                while (null != (line = input.ReadLine()))
                {
                    var newText = $"{line}";
                    if (line.Contains($"// MassTransit Setup -- Do Not Delete Comment"))
                    {
                        newText += $@"
            _provider = services.AddMassTransitInMemoryTestHarness(cfg =>
            {{
                // Consumer Registration -- Do Not Delete Comment
            }}).BuildServiceProvider();
            _harness = _provider.GetRequiredService<InMemoryTestHarness>();

            services.AddScoped(_ => Mock.Of<IPublishEndpoint>());
            await _harness.Start();";
                    }
                    else if (line.Contains($"using System;"))
                    {
                        newText += $@"
    using MassTransit.Testing;
    using MassTransit;";
                    }
                    else if (line.Contains($"// MassTransit Teardown -- Do Not Delete Comment"))
                    {
                        newText += $@"
            await _harness.Stop();";
                    }
                    else if (line.Contains($"// MassTransit Methods -- Do Not Delete Comment"))
                    {
                        newText += $@"
        public static async Task PublishMessage<T>(object message)
            where T : class
        {{
            await _harness.Bus.Publish<T>(message);
        }}";
                    }
                    else if (line.Contains($"private static Checkpoint _checkpoint;"))
                    {
                        newText += $@"
        public static InMemoryTestHarness _harness;
        public static ServiceProvider _provider;";
                    }

                    output.WriteLine(newText);
                }
            }

            // delete the old file and set the name of the new one to the original name
            File.Delete(classPath.FullClassPath);
            File.Move(tempPath, classPath.FullClassPath);
        }

        public static void AddMTConsumer(string testDirectory, string consumerName, string projectBaseName, string srcDirectory)
        {
            var classPath = ClassPathHelper.IntegrationTestProjectRootClassPath(testDirectory, "TestFixture.cs", projectBaseName);

            if (!Directory.Exists(classPath.ClassDirectory))
                throw new DirectoryNotFoundException($"The `{classPath.ClassDirectory}` directory could not be found.");

            if (!File.Exists(classPath.FullClassPath))
                throw new FileNotFoundException($"The `{classPath.FullClassPath}` file could not be found.");

            var consumerClassPath = ClassPathHelper.ConsumerFeaturesClassPath(srcDirectory, $"", projectBaseName);

            var tempPath = $"{classPath.FullClassPath}temp";
            var hasUsingForConsumerNamespace = false;
            using (var input = File.OpenText(classPath.FullClassPath))
            {
                using var output = new StreamWriter(tempPath);
                string line;
                while (null != (line = input.ReadLine()))
                {
                    var newText = $"{line}";
                    if (line.Contains($"// Consumer Registration -- Do Not Delete Comment"))
                    {
                        newText += $@"

                cfg.AddConsumer<{consumerName}>();
                cfg.AddConsumerTestHarness<{consumerName}>();";
                    }
                    if (line.Contains(consumerClassPath.ClassNamespace))
                        hasUsingForConsumerNamespace = true;

                    output.WriteLine(newText);
                }
            }

            // delete the old file and set the name of the new one to the original name
            File.Delete(classPath.FullClassPath);
            File.Move(tempPath, classPath.FullClassPath);

            if (!hasUsingForConsumerNamespace)
            {
                using (var input = File.OpenText(classPath.FullClassPath))
                {
                    using var output = new StreamWriter(tempPath);
                    string line;
                    while (null != (line = input.ReadLine()))
                    {
                        var newText = $"{line}";
                        if (line.Contains($"using MassTransit;"))
                            newText += @$"{Environment.NewLine}    using {consumerClassPath.ClassNamespace};";

                        output.WriteLine(newText);
                    }
                }

                // delete the old file and set the name of the new one to the original name
                File.Delete(classPath.FullClassPath);
                File.Move(tempPath, classPath.FullClassPath);
            }
        }
    }
}