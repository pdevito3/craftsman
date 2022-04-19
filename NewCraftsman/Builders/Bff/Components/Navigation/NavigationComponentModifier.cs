namespace NewCraftsman.Builders.Bff.Components.Navigation
{
    using System;
    using System.IO;
    using System.IO.Abstractions;
    using Helpers;
    using Services;

    public class NavigationComponentModifier
    {
        private readonly IFileSystem _fileSystem;

        public NavigationComponentModifier(IFileSystem fileSystem)
        {
            _fileSystem = fileSystem;
        }

        public void AddFeatureListRouteToNav(string spaDirectory, string entityPlural)
        {
            var classPath = ClassPathHelper.BffSpaComponentClassPath(spaDirectory, "Navigation", "PrivateSideNav.tsx");

            if (!_fileSystem.Directory.Exists(classPath.ClassDirectory))
                _fileSystem.Directory.CreateDirectory(classPath.ClassDirectory);

            if (!_fileSystem.File.Exists(classPath.FullClassPath))
                return; // silently skip this. just want to add this as a convenience if the scaffolding set up is used.

            var tempPath = $"{classPath.FullClassPath}temp";
            using (var input = _fileSystem.File.OpenText(classPath.FullClassPath))
            {
                using var output = _fileSystem.File.CreateText(tempPath);
                {
                    string line;
                    while (null != (line = input.ReadLine()))
                    {
                        var newText = $"{line}";
                        if (line.Contains("/* route marker"))
                            newText += @$"{Environment.NewLine}	{{ name: '{entityPlural.UppercaseFirstLetter()}', href: '/{entityPlural.LowercaseFirstLetter()}', icon: IoFolder }},";
                        

                        output.WriteLine(newText);
                    }
                }
            }

            // delete the old file and set the name of the new one to the original name
            _fileSystem.File.Delete(classPath.FullClassPath);
            _fileSystem.File.Move(tempPath, classPath.FullClassPath);
        }
    }
}
