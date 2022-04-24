namespace Craftsman.Builders.Docker;

using Helpers;
using Services;

public class DockerIgnoreBuilder
{
    private readonly ICraftsmanUtilities _utilities;

    public DockerIgnoreBuilder(ICraftsmanUtilities utilities)
    {
        _utilities = utilities;
    }

    public void CreateDockerIgnore(string srcDirectory, string projectBaseName)
    {
        var classPath = ClassPathHelper.WebApiProjectRootClassPath(srcDirectory, $".dockerignore", projectBaseName);
        var fileText = GetDockerIgnoreText();
        _utilities.CreateFile(classPath, fileText);
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