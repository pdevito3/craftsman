namespace NewCraftsman.Services;

public interface IScaffoldingDirectoryStore
{
    string SolutionDirectory { get; }
    string BoundedContextDirectory { get; }
    string SrcDirectory { get; }
    string TestDirectory { get; }
    string SetSolutionDirectory(string rootDir, string domainName);
    string SetBoundedContextDirectory(string projectName);
}

public class ScaffoldingDirectoryStore : IScaffoldingDirectoryStore
{
    public string SolutionDirectory { get; private set; }
    public string BoundedContextDirectory { get; private set; }
    public string SrcDirectory => Path.Combine(BoundedContextDirectory, "src");
    public string TestDirectory => Path.Combine(BoundedContextDirectory, "tests");
    
    public string SetSolutionDirectory(string rootDir, string domainName)
    {
        SolutionDirectory = $"{rootDir}{Path.DirectorySeparatorChar}{domainName}";
        return SolutionDirectory;
    }
    
    public string SetBoundedContextDirectory(string projectName)
    {
        if(string.IsNullOrEmpty(SolutionDirectory))
            throw new Exception("Invalid Solution Directory");
        
        BoundedContextDirectory = $"{SolutionDirectory}{Path.DirectorySeparatorChar}{projectName}";
        return SolutionDirectory;
    }
}