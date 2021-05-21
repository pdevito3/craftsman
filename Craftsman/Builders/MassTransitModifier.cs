namespace Craftsman.Builders
{
    using Craftsman.Builders.Dtos;
    using Craftsman.Enums;
    using Craftsman.Exceptions;
    using Craftsman.Helpers;
    using Craftsman.Models;
    using System;
    using System.Collections.Generic;
    using System.IO;
    using System.Linq;
    using System.Reflection.Emit;
    using System.Text;
    using static Helpers.ConsoleWriter;

    public class MassTransitModifier
    {
        public static void AddConsumerRegistation(string solutionDirectory, string endpointRegistrationName, string projectBaseName)
        {
            var classPath = ClassPathHelper.WebApiServiceExtensionsClassPath(solutionDirectory, $"{Utilities.GetMassTransitRegistrationName()}.cs", projectBaseName);

            if (!Directory.Exists(classPath.ClassDirectory))
                throw new DirectoryNotFoundException($"The `{classPath.ClassDirectory}` directory could not be found.");

            if (!File.Exists(classPath.FullClassPath))
                throw new FileNotFoundException($"The `{classPath.FullClassPath}` file could not be found.");

            var consumerClassPath = ClassPathHelper.WebApiConsumersServiceExtensionsClassPath(solutionDirectory, $"{endpointRegistrationName}.cs", projectBaseName);

            var tempPath = $"{classPath.FullClassPath}temp";
            var hasUsingForConsumerNamespace = false;
            using (var input = File.OpenText(classPath.FullClassPath))
            {
                using var output = new StreamWriter(tempPath);
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

        public static void AddProducerRegistation(string solutionDirectory, string endpointRegistrationName, string projectBaseName)
        {
            var classPath = ClassPathHelper.WebApiServiceExtensionsClassPath(solutionDirectory, $"{Utilities.GetMassTransitRegistrationName()}.cs", projectBaseName);

            if (!Directory.Exists(classPath.ClassDirectory))
                throw new DirectoryNotFoundException($"The `{classPath.ClassDirectory}` directory could not be found.");

            if (!File.Exists(classPath.FullClassPath))
                throw new FileNotFoundException($"The `{classPath.FullClassPath}` file could not be found.");

            var producerClassPath = ClassPathHelper.WebApiProducersServiceExtensionsClassPath(solutionDirectory, $"{endpointRegistrationName}.cs", projectBaseName);

            var tempPath = $"{classPath.FullClassPath}temp";
            var hasUsingForProducerNamespace = false;
            using (var input = File.OpenText(classPath.FullClassPath))
            {
                using var output = new StreamWriter(tempPath);
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
            File.Delete(classPath.FullClassPath);
            File.Move(tempPath, classPath.FullClassPath);

            if (!hasUsingForProducerNamespace)
            {
                using (var input = File.OpenText(classPath.FullClassPath))
                {
                    using var output = new StreamWriter(tempPath);
                    string line;
                    while (null != (line = input.ReadLine()))
                    {
                        var newText = $"{line}";
                        if (line.Contains($"using MassTransit;"))
                            newText += @$"{Environment.NewLine}    using {producerClassPath.ClassNamespace};";

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