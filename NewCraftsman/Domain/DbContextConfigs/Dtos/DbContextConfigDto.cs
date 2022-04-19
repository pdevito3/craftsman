namespace NewCraftsman.Domain.DbContextConfigs.Dtos;

public class DbContextConfigDto
{
    public string ContextName { get; set; }
    public string DatabaseName { get; set; }
    public string Provider { get; set; }
    public string NamingConvention { get; set; }
}