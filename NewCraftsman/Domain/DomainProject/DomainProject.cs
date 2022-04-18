namespace NewCraftsman.Domain.DomainProject;

using AutoMapper;
using Dtos;
using FluentValidation;
using Mappings;
using Validators;

public class DomainProject
{
    public string DomainName { get; private set; }

    // public List<ApiTemplate> BoundedContexts { get; private set; }

    public bool AddGit { get; private set; }

    public bool UseSystemGitUser { get; private set; }

    /// <summary>
    /// A list of eventing messages to be added to the domain
    /// </summary>
    // public List<Message> Messages { get; private set; } = new List<Message>();
    //
    // public AuthServerTemplate AuthServer { get; private set; } = null;
    //     
    // public BffTemplate Bff { get; private set; } = null;

    public static DomainProject Create(DomainProjectDto domainProjectDto)
    {
        domainProjectDto.AddGit ??= true;
        domainProjectDto.UseSystemGitUser ??= true;
        
        new DomainProjectDtoValidator().ValidateAndThrow(domainProjectDto);
        var mapper = new Mapper(new MapperConfiguration(cfg => {
            cfg.AddProfile<DomainProjectProfile>();
        }));
        var newDomainProject = mapper.Map<DomainProject>(domainProjectDto);
        
        return newDomainProject;
    }
}