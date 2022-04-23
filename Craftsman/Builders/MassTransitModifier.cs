namespace Craftsman.Builders
{
    using System;
    using System.IO;
    using System.IO.Abstractions;
    using Services;

    public class MassTransitModifier
    {
        private readonly IFileSystem _fileSystem;

        public MassTransitModifier(IFileSystem fileSystem)
        {
            _fileSystem = fileSystem;
        }

        public void AddConsumerRegistation(string solutionDirectory, string endpointRegistrationName, string projectBaseName)
        {
            var classPath = ClassPathHelper.WebApiServiceExtensionsClassPath(solutionDirectory, $"{FileNames.GetMassTransitRegistrationName()}.cs", projectBaseName);

            if (!_fileSystem.Directory.Exists(classPath.ClassDirectory))
                _fileSystem.Directory.CreateDirectory(classPath.ClassDirectory);

            if (!_fileSystem.File.Exists(classPath.FullClassPath))
                throw new FileNotFoundException($"The `{classPath.FullClassPath}` file could not be found.");

            var consumerClassPath = ClassPathHelper.WebApiConsumersServiceExtensionsClassPath(solutionDirectory, $"{endpointRegistrationName}.cs", projectBaseName);

            var hasUsingForConsumerNamespace = false;
            
            var tempPath = $"{classPath.FullClassPath}temp";
            using (var input = _fileSystem.File.OpenText(classPath.FullClassPath))
            {
                using var output = _fileSystem.File.CreateText(tempPath);
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

        public void AddProducerRegistration(string solutionDirectory, string endpointRegistrationName, string projectBaseName)
        {
            var classPath = ClassPathHelper.WebApiServiceExtensionsClassPath(solutionDirectory, $"{FileNames.GetMassTransitRegistrationName()}.cs", projectBaseName);

            if (!_fileSystem.Directory.Exists(classPath.ClassDirectory))
                throw new DirectoryNotFoundException($"The `{classPath.ClassDirectory}` directory could not be found.");

            if (!_fileSystem.File.Exists(classPath.FullClassPath))
                throw new FileNotFoundException($"The `{classPath.FullClassPath}` file could not be found.");

            var producerClassPath = ClassPathHelper.WebApiProducersServiceExtensionsClassPath(solutionDirectory, $"{endpointRegistrationName}.cs", projectBaseName);

            var tempPath = $"{classPath.FullClassPath}temp";
            var hasUsingForProducerNamespace = false;
            using (var input = _fileSystem.File.OpenText(classPath.FullClassPath))
            {
                using var output = _fileSystem.File.CreateText(tempPath);
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
            _fileSystem.File.Delete(classPath.FullClassPath);
            _fileSystem.File.Move(tempPath, classPath.FullClassPath);

            if (!hasUsingForProducerNamespace)
            {
                using (var input = _fileSystem.File.OpenText(classPath.FullClassPath))
                {
                    using var output = _fileSystem.File.CreateText(tempPath);
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
                _fileSystem.File.Delete(classPath.FullClassPath);
                _fileSystem.File.Move(tempPath, classPath.FullClassPath);
            }
        }
    }
}