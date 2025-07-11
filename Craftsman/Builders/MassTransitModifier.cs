namespace Craftsman.Builders;

using System;
using System.IO;
using System.IO.Abstractions;
using Services;
using MediatR;

public static class MassTransitModifier
{
    public sealed record AddConsumerRegistrationCommand(string EndpointRegistrationName) : IRequest;
    public sealed record AddProducerRegistrationCommand(string EndpointRegistrationName) : IRequest;

    public class AddConsumerRegistrationHandler(IFileSystem fileSystem, IScaffoldingDirectoryStore scaffoldingDirectoryStore) : IRequestHandler<AddConsumerRegistrationCommand>
    {
        public Task Handle(AddConsumerRegistrationCommand request, CancellationToken cancellationToken)
        {
            AddConsumerRegistration(scaffoldingDirectoryStore.SrcDirectory, request.EndpointRegistrationName, scaffoldingDirectoryStore.ProjectBaseName, fileSystem);
            return Task.CompletedTask;
        }
    }

    public class AddProducerRegistrationHandler(IFileSystem fileSystem, IScaffoldingDirectoryStore scaffoldingDirectoryStore) : IRequestHandler<AddProducerRegistrationCommand>
    {
        public Task Handle(AddProducerRegistrationCommand request, CancellationToken cancellationToken)
        {
            AddProducerRegistration(scaffoldingDirectoryStore.SrcDirectory, request.EndpointRegistrationName, scaffoldingDirectoryStore.ProjectBaseName, fileSystem);
            return Task.CompletedTask;
        }
    }

    private static void AddConsumerRegistration(string solutionDirectory, string endpointRegistrationName, string projectBaseName, IFileSystem fileSystem)
    {
        var classPath = ClassPathHelper.WebApiServiceExtensionsClassPath(solutionDirectory, $"{FileNames.GetMassTransitRegistrationName()}.cs", projectBaseName);

        if (!fileSystem.Directory.Exists(classPath.ClassDirectory))
            fileSystem.Directory.CreateDirectory(classPath.ClassDirectory);

        if (!fileSystem.File.Exists(classPath.FullClassPath))
            throw new FileNotFoundException($"The `{classPath.FullClassPath}` file could not be found.");

        var consumerClassPath = ClassPathHelper.WebApiConsumersServiceExtensionsClassPath(solutionDirectory, $"{endpointRegistrationName}.cs", projectBaseName);

        var hasUsingForConsumerNamespace = false;

        var tempPath = $"{classPath.FullClassPath}temp";
        using (var input = fileSystem.File.OpenText(classPath.FullClassPath))
        {
            using var output = fileSystem.File.CreateText(tempPath);
            string line;
            while (null != (line = input.ReadLine()))
            {
                var newText = $"{line}";
                if (line.Contains($"// Consumers -- Do Not Delete This Comment"))
                    newText += @$"{Environment.NewLine}                    cfg.{endpointRegistrationName}(context);";
                if (line.Contains(consumerClassPath.ClassNamespace))
                    hasUsingForConsumerNamespace = true;

                output.WriteLine(newText);
            }
        }

        // delete the old file and set the name of the new one to the original name
        fileSystem.File.Delete(classPath.FullClassPath);
        fileSystem.File.Move(tempPath, classPath.FullClassPath);

        if (!hasUsingForConsumerNamespace)
        {
            using (var input = fileSystem.File.OpenText(classPath.FullClassPath))
            {
                using var output = fileSystem.File.CreateText(tempPath);
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
            fileSystem.File.Delete(classPath.FullClassPath);
            fileSystem.File.Move(tempPath, classPath.FullClassPath);
        }
    }

    private static void AddProducerRegistration(string solutionDirectory, string endpointRegistrationName, string projectBaseName, IFileSystem fileSystem)
    {
        var classPath = ClassPathHelper.WebApiServiceExtensionsClassPath(solutionDirectory, $"{FileNames.GetMassTransitRegistrationName()}.cs", projectBaseName);

        if (!fileSystem.Directory.Exists(classPath.ClassDirectory))
            throw new DirectoryNotFoundException($"The `{classPath.ClassDirectory}` directory could not be found.");

        if (!fileSystem.File.Exists(classPath.FullClassPath))
            throw new FileNotFoundException($"The `{classPath.FullClassPath}` file could not be found.");

        var producerClassPath = ClassPathHelper.WebApiProducersServiceExtensionsClassPath(solutionDirectory, $"{endpointRegistrationName}.cs", projectBaseName);

        var tempPath = $"{classPath.FullClassPath}temp";
        var hasUsingForProducerNamespace = false;
        using (var input = fileSystem.File.OpenText(classPath.FullClassPath))
        {
            using var output = fileSystem.File.CreateText(tempPath);
            string line;
            while (null != (line = input.ReadLine()))
            {
                var newText = $"{line}";
                if (line.Contains($"// Producers -- Do Not Delete This Comment"))
                    newText += @$"{Environment.NewLine}                    cfg.{endpointRegistrationName}();";
                if (line.Contains(producerClassPath.ClassNamespace))
                    hasUsingForProducerNamespace = true;

                output.WriteLine(newText);
            }
        }

        // delete the old file and set the name of the new one to the original name
        fileSystem.File.Delete(classPath.FullClassPath);
        fileSystem.File.Move(tempPath, classPath.FullClassPath);

        if (!hasUsingForProducerNamespace)
        {
            using (var input = fileSystem.File.OpenText(classPath.FullClassPath))
            {
                using var output = fileSystem.File.CreateText(tempPath);
                string line;
                while (null != (line = input.ReadLine()))
                {
                    var newText = $"{line}";
                    if (line.Contains($"using MassTransit;"))
                        newText += @$"{Environment.NewLine}using {producerClassPath.ClassNamespace};";

                    output.WriteLine(newText);
                }
            }

            // delete the old file and set the name of the new one to the original name
            fileSystem.File.Delete(classPath.FullClassPath);
            fileSystem.File.Move(tempPath, classPath.FullClassPath);
        }
    }
}
