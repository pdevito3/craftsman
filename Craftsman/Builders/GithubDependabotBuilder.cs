namespace Craftsman.Builders;

using Helpers;
using Services;

public class GithubDependabotBuilder
{
    private readonly ICraftsmanUtilities _utilities;

    public GithubDependabotBuilder(ICraftsmanUtilities utilities)
    {
        _utilities = utilities;
    }

    public void CreateFile(string solutionDirectory)
    {
        var classPath = ClassPathHelper.GithubClassPath(solutionDirectory, $"dependabot.yaml");
        var fileText = GetFileText();
        _utilities.CreateFile(classPath, fileText);
    }

    public static string GetFileText()
    {
        return @$"version: 2

updates:
  - package-ecosystem: ""nuget""
    # Targeted directory, it will look for any csProj file recursively.
    directory: ""/""
    schedule:
      interval: ""weekly""
      # wednesday so we can get the latest .net release from tuesday on the weeks it does drop
      day: ""wednesday""
    commit-message:      
      prefix: ""Package Dependencies""
    # Temporarily disable PR limit, till initial dependency update goes through
    open-pull-requests-limit: 1000";
    }
}
