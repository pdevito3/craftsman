namespace Craftsman.Builders.Bff.Src;

using System;
using System.IO.Abstractions;
using Helpers;
using Services;

public class DynamicFeatureRoutesModifier
{
    private readonly IFileSystem _fileSystem;

    public DynamicFeatureRoutesModifier(IFileSystem fileSystem)
    {
        _fileSystem = fileSystem;
    }

    public void AddRoute(string spaDirectory, string entityName, string entityPlural)
    {
        var classPath = ClassPathHelper.BffSpaSrcClassPath(spaDirectory, "App.tsx");

        if (!_fileSystem.Directory.Exists(classPath.ClassDirectory))
            _fileSystem.Directory.CreateDirectory(classPath.ClassDirectory);

        if (!_fileSystem.File.Exists(classPath.FullClassPath))
            return; // silently skip this. just want to add this as a convenience if the scaffolding set up is used.

        var listRouteName = FileNames.BffEntityListRouteComponentName(entityName);
        var haveAddedImport = false;
        var tempPath = $"{classPath.FullClassPath}temp";
        using (var input = _fileSystem.File.OpenText(classPath.FullClassPath))
        {
            using var output = _fileSystem.File.CreateText(tempPath);
            {
                string line;
                while (null != (line = input.ReadLine()))
                {
                    var newText = $"{line}";
                    if (line.Contains("import") && !haveAddedImport)
                    {
                        newText += @$"{Environment.NewLine}import {{ {listRouteName} }} from './features/{entityPlural}';";
                        haveAddedImport = true;
                    }
                    if (line.Contains("/* route marker"))
                    {
                        newText += @$"{Environment.NewLine}                            <Route path=""/{entityPlural.LowercaseFirstLetter()}"" element={{<{listRouteName} />}} />";
                    }

                    output.WriteLine(newText);
                }
            }
        }

        // delete the old file and set the name of the new one to the original name
        _fileSystem.File.Delete(classPath.FullClassPath);
        _fileSystem.File.Move(tempPath, classPath.FullClassPath);
    }
}
