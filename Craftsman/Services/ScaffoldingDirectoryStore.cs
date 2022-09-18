namespace Craftsman.Services;

public interface IScaffoldingDirectoryStore
{
    string SolutionDirectory { get; }
    public string SpaDirectory { get; }
    public string SpaSrcDirectory { get; }
    string BoundedContextDirectory { get; }
    string SrcDirectory { get; }
    string TestDirectory { get; }
    string ProjectBaseName { get; }
    string SetSolutionDirectory(string rootDir, string domainName);
    string SetSolutionDirectory(string solutionDir);
    string SetBoundedContextDirectoryAndProject(string projectName);
    string SetNextJsDir(string projectName);
    void SetSpaDirectory(string spaDirectory);
}

public class ScaffoldingDirectoryStore : IScaffoldingDirectoryStore
{
    public string SolutionDirectory { get; private set; }
    public string SpaDirectory { get; private set; }
    public string SpaSrcDirectory => string.IsNullOrEmpty(SpaDirectory)
        ? null
        : Path.Combine(SpaDirectory, "src");
    public string BoundedContextDirectory { get; private set; }
    public string ProjectBaseName { get; private set; }
    public string SrcDirectory => string.IsNullOrEmpty(BoundedContextDirectory)
        ? null
        : Path.Combine(BoundedContextDirectory, "src");
    public string TestDirectory => string.IsNullOrEmpty(BoundedContextDirectory)
        ? null
        : Path.Combine(BoundedContextDirectory, "tests");

    public string SetSolutionDirectory(string rootDir, string domainName)
    {
        if (string.IsNullOrEmpty(rootDir))
            throw new Exception("Invalid Root Directory");
        if (string.IsNullOrEmpty(domainName))
            throw new Exception("Invalid Domain Name");

        SetSolutionDirectory(Path.Combine(rootDir, domainName));
        return SolutionDirectory;
    }

    public string SetSolutionDirectory(string solutionDir)
    {
        if (string.IsNullOrEmpty(solutionDir))
            throw new Exception("Invalid Solution Directory");

        SolutionDirectory = solutionDir;
        return SolutionDirectory;
    }

    public string SetBoundedContextDirectoryAndProject(string projectName)
    {
        if (string.IsNullOrEmpty(SolutionDirectory))
            throw new Exception("Invalid Solution Directory");

        ProjectBaseName = projectName;
        BoundedContextDirectory = Path.Combine(SolutionDirectory, projectName);
        return SolutionDirectory;
    }

    public string SetNextJsDir(string nextJsRootDir)
    {
        SpaDirectory = nextJsRootDir;
        return SpaDirectory;
    }

    public void SetSpaDirectory(string spaDirectory)
    {
        SpaDirectory = spaDirectory;
    }
}