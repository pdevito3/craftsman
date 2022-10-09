namespace Craftsman.Builders.AuthServer;

using Craftsman.Helpers;
using Craftsman.Services;

public class ProgramBuilder
{
    private readonly ICraftsmanUtilities _utilities;

    public ProgramBuilder(ICraftsmanUtilities utilities)
    {
        _utilities = utilities;
    }

    public void CreateAuthServerProgram(string solutionDirectory, string authServerProjectName)
    {
        var classPath = ClassPathHelper.WebApiProjectRootClassPath(solutionDirectory, $"Program.cs", authServerProjectName);
        var fileText = GetAuthServerProgramText(classPath.ClassNamespace);
        _utilities.CreateFile(classPath, fileText);
    }

    public static string GetAuthServerProgramText(string classNamespace)
    {
        return @$"namespace {classNamespace};

using System.Threading.Tasks;
using Pulumi;

internal static class Program
{{
    private static Task<int> Main() => Deployment.RunAsync<RealmBuild>();
}}";
    }
}
