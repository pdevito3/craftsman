namespace Craftsman.Builders;

using System;
using System.IO;
using System.IO.Abstractions;
using Services;

public class ProgramModifier
{
    private readonly IFileSystem _fileSystem;

    public ProgramModifier(IFileSystem fileSystem)
    {
        _fileSystem = fileSystem;
    }

    public void RegisterMassTransitService(string srcDirectory, string projectBaseName)
    {
        var classPath = ClassPathHelper.WebApiServiceExtensionsClassPath(srcDirectory, $"{FileNames.WebAppServiceConfiguration()}.cs", projectBaseName);

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
                if (line.Contains($"services.AddInfrastructure"))
                    newText += @$"{Environment.NewLine}        builder.Services.AddMassTransitServices(builder.Environment);";

                //if (line.Contains($@"{infraClassPath.ClassNamespace};"))
                //    newText += @$"{ Environment.NewLine}    using { serviceRegistrationsClassPath.ClassNamespace}; ";

                output.WriteLine(newText);
            }
        }

        // delete the old file and set the name of the new one to the original name
        _fileSystem.File.Delete(classPath.FullClassPath);
        _fileSystem.File.Move(tempPath, classPath.FullClassPath);
    }
}
