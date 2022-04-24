namespace Craftsman.Domain;

public abstract class BffBased
{
    private string _projectName;
    public string ProjectName
    {
        get => _projectName ?? throw new Exception("The project name is required to target the BFF.");
        set => _projectName = value;
    }

    public string GetProjectDirectory(string domainDirectory)
    {
        return $"{domainDirectory}{Path.DirectorySeparatorChar}{ProjectName}";
    }

    public string GetSpaDirectory(string domainDirectory)
    {
        return Path.Combine(GetProjectDirectory(domainDirectory), "ClientApp");
    }
}