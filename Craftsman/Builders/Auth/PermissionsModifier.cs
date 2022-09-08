namespace Craftsman.Builders.Auth;

using System.IO.Abstractions;
using Services;

public class PermissionsModifier
{
    private readonly IFileSystem _fileSystem;

    public PermissionsModifier(IFileSystem fileSystem)
    {
        _fileSystem = fileSystem;
    }

    public void AddPermission(string srcDirectory, string permission, string projectBaseName)
    {
        var classPath = ClassPathHelper.PolicyDomainClassPath(srcDirectory, $"Permissions.cs", projectBaseName);

        if (!_fileSystem.Directory.Exists(classPath.ClassDirectory))
            _fileSystem.Directory.CreateDirectory(classPath.ClassDirectory);

        if (!_fileSystem.File.Exists(classPath.FullClassPath))
            throw new FileNotFoundException($"The `{classPath.FullClassPath}` file could not be found.");

        var fileText = _fileSystem.File.ReadAllText(classPath.FullClassPath);
        if (fileText.Contains($"const string {permission} ="))
            return;

        var tempPath = $"{classPath.FullClassPath}temp";
        using (var input = _fileSystem.File.OpenText(classPath.FullClassPath))
        {
            using var output = _fileSystem.File.CreateText(tempPath);
            string line;
            while (null != (line = input.ReadLine()))
            {
                var newText = $"{line}";
                if (line.Contains($"Permissions marker"))
                {
                    newText += @$"{Environment.NewLine}    public const string {permission} = nameof({permission});";
                }

                output.WriteLine(newText);
            }
        }

        // delete the old file and set the name of the new one to the original name
        _fileSystem.File.Delete(classPath.FullClassPath);
        _fileSystem.File.Move(tempPath, classPath.FullClassPath);
    }
}

