namespace NewCraftsman.Domain.BoundedContexts;

using AutoMapper;
using DbContextConfigs;
using DbContextConfigs.Mappings;
using Dtos;
using FluentValidation;
using Mappings;
using Validators;

/// <summary>
/// This is the complete object representation of the API that we will read in from our input file and scaffold out the necessary files
/// </summary>
public class BoundedContext
{
    /// <summary>
    /// The name of the project in your bounded context
    /// </summary>
    public string ProjectName { get; private set; }
    
    /// <summary>
    /// The port that will be used when running locally in the project.
    /// </summary>
    public int Port { get; private set; } // TODO set to find free port

    /// <summary>
    /// The name of the solution you want to build
    /// </summary>
    public DbContextConfig DbContext { get; private set; } = new DbContextConfig();

    // /// <summary>
    // /// Complete list of database entities
    // /// </summary>
    // public List<Entity> Entities { get; private set; } = new List<Entity>();
    //
    // /// <summary>
    // /// Layout of the swagger configuration for the API. Optional
    // /// </summary>
    // public SwaggerConfig SwaggerConfig { get; private set; } = new SwaggerConfig();
    //
    // /// <summary>
    // /// List of each environment to add into the API. Optional
    // /// </summary>
    // public ApiEnvironment Environment { get; private set; } = new ApiEnvironment();
    //
    // /// <summary>
    // /// Calculation to determine whether or not authentication is added to the project
    // /// </summary>
    // public bool AddJwtAuthentication => Environment?.AuthSettings?.Authority?.Length > 0;
    //
    // private Bus _bus = new();
    // /// <summary>
    // /// Message bus information for the bounded context. **Environment will be overriden by the BC environment and should be set there**
    // /// </summary>
    // public Bus Bus
    // {
    //     get
    //     {
    //         _bus.Environment =
    //             Environment; // get bus environment settings from domain environments for a single source of truth
    //         return _bus;
    //     }
    //     set => _bus = value;
    // }
    //
    // /// <summary>
    // /// A list of eventing consumers to be added to the BC
    // /// </summary>
    // public List<Consumer> Consumers { get; private set; } = new List<Consumer>();
    //
    // /// <summary>
    // /// A list of eventing producers to be added to the BC
    // /// </summary>
    // public List<Producer> Producers { get; private set; } = new List<Producer>();
    //
    // /// <summary>
    // /// The value used for setting the policy name in your swagger config to be used for the scope that has access to the given boundary.
    // /// </summary>
    // private string _policyName;
    //
    // public string PolicyName
    // {
    //     get => _policyName ?? ProjectName.Underscore();
    //     set => _policyName = value;
    // }
    //
    // public bool UseSoftDelete { get; private set; } = true;
    //
    // public DockerConfig DockerConfig { get; private set; } = new DockerConfig();
    
    

    public static BoundedContext Create(BoundedContextDto boundedContextDto)
    {
        boundedContextDto.Port ??= 5000;
        
        new BoundedContextDtoValidator().ValidateAndThrow(boundedContextDto);
        var mapper = new Mapper(new MapperConfiguration(cfg =>
        {
            cfg.AddProfile<BoundedContextProfile>();
            cfg.AddProfile<DbContextConfigProfile>();
        }));
        
        var newBoundedContext = mapper.Map<BoundedContext>(boundedContextDto);

        return newBoundedContext;
    }
}

