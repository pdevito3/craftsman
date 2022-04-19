namespace NewCraftsman.Domain.BoundedContexts.Dtos;

using DbContextConfigs.Dtos;

public class BoundedContextDto
{
    public string ProjectName { get; set; }
    
    public int? Port { get; set; }

    public DbContextConfigDto DbContext { get; set; } = new DbContextConfigDto();

    // public List<Entity> Entities { get; set; } = new List<Entity>();
    //
    // public SwaggerConfig SwaggerConfig { get; set; } = new SwaggerConfig();
    //
    // public ApiEnvironment Environment { get; set; } = new ApiEnvironment();
    //
    // public Bus Bus { get; set; }
    //
    // public List<Consumer> Consumers { get; set; } = new List<Consumer>();
    //
    // public List<Producer> Producers { get; set; } = new List<Producer>();
    //
    // public string PolicyName { get; set; }
    //
    // public bool UseSoftDelete { get; set; }
    //
    // public DockerConfig DockerConfig { get; set; } = new DockerConfig();
}

