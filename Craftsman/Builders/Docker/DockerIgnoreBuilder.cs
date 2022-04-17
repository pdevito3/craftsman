namespace Craftsman.Builders.Docker;

using System.IO.Abstractions;
using Helpers;

public static class DockerIgnoreBuilder
{
    public static void CreateDockerIgnore(string srcDirectory, string projectBaseName, IFileSystem fileSystem)
    {
        var classPath = ClassPathHelper.WebApiProjectRootClassPath(srcDirectory, $".dockerignore", projectBaseName);
        var fileText = GetDockerIgnoreText();
        Utilities.CreateFile(classPath, fileText, fileSystem);
    }
    
    private static string GetDockerIgnoreText()
    {
        return @$"**/.dockerignore
**/.env
**/.git
**/.gitignore
**/.project
**/.settings
**/.toolstarget
**/.vs
**/.vscode
**/.idea
**/*.*proj.user
**/*.dbmdl
**/*.jfm
**/azds.yaml
**/bin
**/charts
**/docker-compose*
**/Dockerfile*
**/node_modules
**/npm-debug.log
**/obj
**/secrets.dev.yaml
**/values.dev.yaml
LICENSE
README.md";
    }
}