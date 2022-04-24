namespace Craftsman.Services;

using System.IO.Abstractions;
using Builders;
using LibGit2Sharp;

public interface IGitService
{
    void GitSetup(string solutionDirectory, bool useSystemGitUser);
}

public class GitService : IGitService
{
    private readonly IFileSystem _fileSystem;

    public GitService(IFileSystem fileSystem)
    {
        _fileSystem = fileSystem;
    }

    public void GitSetup(string solutionDirectory, bool useSystemGitUser)
    {
        new GitBuilder(_fileSystem).CreateGitIgnore(solutionDirectory);

        Repository.Init(solutionDirectory);
        var repo = new Repository(solutionDirectory);

        string[] allFiles = Directory.GetFiles(solutionDirectory, "*.*", SearchOption.AllDirectories);
        Commands.Stage(repo, allFiles);

        var author = useSystemGitUser
            ? repo.Config.BuildSignature(DateTimeOffset.Now)
            : new Signature("Craftsman", "craftsman", DateTimeOffset.Now);
        repo.Commit("Initial Commit", author, author);
    }
}