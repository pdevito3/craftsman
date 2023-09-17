namespace Craftsman.Services;

using System.IO.Abstractions;
using Builders;
using Helpers;
using LibGit2Sharp;

public interface IGitService
{
    void GitSetup(string solutionDirectory, bool useSystemGitUser);
}

public class GitService : IGitService
{
    private readonly IFileSystem _fileSystem;
    private readonly IConsoleWriter _consoleWriter;

    public GitService(IFileSystem fileSystem, IConsoleWriter consoleWriter)
    {
        _fileSystem = fileSystem;
        _consoleWriter = consoleWriter;
    }

    public void GitSetup(string solutionDirectory, bool useSystemGitUser)
    {
        new GitBuilder(_fileSystem).CreateGitIgnore(solutionDirectory);

        Repository.Init(solutionDirectory);
        var repo = new Repository(solutionDirectory);

        string[] allFiles = Directory.GetFiles(solutionDirectory, "*.*", SearchOption.AllDirectories);
        Commands.Stage(repo, allFiles);

        var author = new Signature("Craftsman", "craftsman", DateTimeOffset.Now);

        if (useSystemGitUser)
        {
            var systemUser = repo.Config.BuildSignature(DateTimeOffset.Now);
            if (systemUser == null)
            {
                _consoleWriter.WriteWarning(@$"You are attempting to use the system git user, but a system git user could not be found. Please ensure you have a system git user configured. 

You can configure a username and email with `git config --global user.name ""My Name""` and `git config --global user.email ""myname@email.com""`.

Falling back to the Craftsman user.");
            }
            else
            {
                author = systemUser;
            }
        }
        repo.Commit("Initial Commit", author, author);
    }
}