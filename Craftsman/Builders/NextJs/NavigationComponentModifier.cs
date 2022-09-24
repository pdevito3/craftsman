namespace Craftsman.Builders.NextJs;

using System;
using System.IO.Abstractions;
using Craftsman.Helpers;
using Craftsman.Services;

public class NavigationComponentModifier
{
    private readonly IFileSystem _fileSystem;

    public NavigationComponentModifier(IFileSystem fileSystem)
    {
        _fileSystem = fileSystem;
    }

    public void AddFeatureListRouteToNav(string nextSrcDirectory, string entityPlural, string icon)
    {
        var classPath = ClassPathHelper.NextJsSideNavClassPath(nextSrcDirectory);

        if (!_fileSystem.Directory.Exists(classPath.ClassDirectory))
            _fileSystem.Directory.CreateDirectory(classPath.ClassDirectory);

        if (!_fileSystem.File.Exists(classPath.FullClassPath))
            return; // silently skip this. just want to add this as a convenience if the scaffolding set up is used.

        var tempPath = $"{classPath.FullClassPath}temp";
        var iconExists = false;
        using (var input = _fileSystem.File.OpenText(classPath.FullClassPath))
        {
            using var output = _fileSystem.File.CreateText(tempPath);
            {
                string line;
                while (null != (line = input.ReadLine()))
                {
                    var newText = $"{line}";
                    if (line.Contains($"{icon},"))
                        iconExists = true;
                    if (line.Contains("/* route marker"))
                        newText += @$"{Environment.NewLine}	{{ name: '{entityPlural.UppercaseFirstLetter()}', href: '/{entityPlural.ToLower()}', icon: {icon} }},";
                    if (line.Contains(@$"}} from ""@tabler/icons""") && !iconExists)
                        newText = @$"  {icon},{Environment.NewLine}{line}";

                    output.WriteLine(newText);
                }
            }
        }

        // delete the old file and set the name of the new one to the original name
        _fileSystem.File.Delete(classPath.FullClassPath);
        _fileSystem.File.Move(tempPath, classPath.FullClassPath);
    }
}
