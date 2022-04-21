namespace NewCraftsman.Services;

public interface IScaffoldingDirectoryStore
{
    string SolutionDirectory { get; }
    string BoundedContextDirectory { get; }
    string SrcDirectory { get; }
    string TestDirectory { get; }
    string ProjectBaseName { get; }
    string SetSolutionDirectory(string rootDir, string domainName);
    string SetSolutionDirectory(string solutionDir);
    string SetBoundedContextDirectoryAndProject(string projectName);
}

public class ScaffoldingDirectoryStore : IScaffoldingDirectoryStore
{
    public string SolutionDirectory { get; private set; }
    public string BoundedContextDirectory { get; private set; }
    public string ProjectBaseName { get; private set; }
    public string SrcDirectory => Path.Combine(BoundedContextDirectory, "src");
    public string TestDirectory => Path.Combine(BoundedContextDirectory, "tests");
    
    public string SetSolutionDirectory(string rootDir, string domainName)
    {
        if(string.IsNullOrEmpty(rootDir))
            throw new Exception("Invalid Root Directory");
        if(string.IsNullOrEmpty(domainName))
            throw new Exception("Invalid Domain Name");
        
        SetSolutionDirectory($"{rootDir}{Path.DirectorySeparatorChar}{domainName}");
        return SolutionDirectory;
    }

    public string SetSolutionDirectory(string solutionDir)
    {
        if(string.IsNullOrEmpty(solutionDir))
            throw new Exception("Invalid Solution Directory");
        
        SolutionDirectory = solutionDir;
        return SolutionDirectory;
    }

    public string SetBoundedContextDirectoryAndProject(string projectName)
    {
        if(string.IsNullOrEmpty(SolutionDirectory))
            throw new Exception("Invalid Solution Directory");

        ProjectBaseName = projectName;
        BoundedContextDirectory = $"{SolutionDirectory}{Path.DirectorySeparatorChar}{projectName}";
        return SolutionDirectory;
    }
    
}