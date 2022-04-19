namespace NewCraftsman.Domain.DomainProjects.Dtos;

using BoundedContexts.Dtos;

public class DomainProjectDto
{
    public string DomainName { get; set; }

    public List<BoundedContextDto> BoundedContexts { get; set; } = new List<BoundedContextDto>();

    public bool? AddGit { get; set; } = null;

    public bool? UseSystemGitUser { get; set; } = null;

    /// <summary>
    /// A list of eventing messages to be added to the domain
    /// </summary>
    // public List<Message> Messages { get; set; } = new List<Message>();
    //
    // public AuthServerTemplate AuthServer { get; set; } = null;
    //     
    // public BffTemplate Bff { get; set; } = null;
    
}