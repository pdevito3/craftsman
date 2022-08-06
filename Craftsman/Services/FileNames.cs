namespace Craftsman.Services;

using Domain;
using Domain.Enums;
using Helpers;

public static class FileNames
{
    public static string BoundaryServiceInterface(string projectBaseName)
    {
        return $"I{projectBaseName}Service";
    }
    
    public static string EntityRepository(string entityName)
    {
        return $"{entityName.UppercaseFirstLetter()}Repository";
    }
    
    public static string EntityRepositoryInterface(string entityName)
    {
        return $"I{EntityRepository(entityName)}";
    }
    
    public static string GenericRepository()
    {
        return $"GenericRepository";
    }
    
    public static string GenericRepositoryInterface()
    {
        return $"I{GenericRepository()}";
    }
    
    public static string WebAppServiceConfiguration()
    {
        return $"WebAppServiceConfiguration";
    }
    
    public static string GetMassTransitRegistrationName()
    {
        return "MassTransitServiceExtension";
    }

    public static string MessageClassName(string messageName)
    {
        return $"{messageName}";
    }

    public static string MessageInterfaceName(string messageName)
    {
        return $"I{messageName}";
    }

    public static string EntityCreatedDomainMessage(string entityName)
    {
        return $"{entityName}Created";
    }

    public static string EntityUpdatedDomainMessage(string entityName)
    {
        return $"{entityName}Updated";
    }

    public static string GetApiRouteClass(string entityPlural)
    {
        return entityPlural;
    }

    public static string GetWebHostFactoryName()
    {
        return "TestingWebApplicationFactory";
    }

    public static string GetControllerName(string entityName)
    {
        return $"{entityName}Controller";
    }

    public static string GetDatabaseEntityConfigName(string entityName)
    {
        return $"{entityName}Configuration";
    }

    public static string GetSeederName(Entity entity)
    {
        return $"{entity.Name}Seeder";
    }

    public static string GetInfraRegistrationName()
    {
        return "InfrastructureServiceExtension";
    }

    public static string GetSwaggerServiceExtensionName()
    {
        return "SwaggerServiceExtension";
    }

    public static string GetAppSettingsName(bool asJson = true)
    {
        return asJson ? $"appsettings.json" : $"appsettings";
    }

    public static string BffApiKeysFilename(string entityName)
    {
        return $"{entityName.LowercaseFirstLetter()}.keys";
    }

    public static string BffEntityListRouteComponentName(string entityName)
    {
        return $"{entityName.UppercaseFirstLetter()}List";
    }

    public static string BffApiKeysExport(string entityName)
    {
        return $"{entityName.UppercaseFirstLetter()}Keys";
    }

    public static string GetMappingName(string entityName)
    {
        return $"{entityName}Mappings";
    }

    public static string GetIntegrationTestFixtureName()
    {
        return $"TestFixture";
    }

    public static string CreateEntityUnitTestName(string entityName)
    {
        return $"Create{entityName}Tests";
    }

    public static string GetEntityListUnitTestName(string entityName)
    {
        return $"Get{entityName}ListTests";
    }

    public static string UpdateEntityUnitTestName(string entityName)
    {
        return $"Update{entityName}Tests";
    }

    public static string GetEntityFeatureClassName(string entityName)
    {
        return $"Get{entityName}";
    }

    public static string GetEntityListFeatureClassName(string entityName)
    {
        return $"Get{entityName}List";
    }

    public static string AddEntityFeatureClassName(string entityName)
    {
        return $"Add{entityName}";
    }

    public static string DeleteEntityFeatureClassName(string entityName)
    {
        return $"Delete{entityName}";
    }

    public static string UpdateEntityFeatureClassName(string entityName)
    {
        return $"Update{entityName}";
    }

    public static string PatchEntityFeatureClassName(string entityName)
    {
        return $"Patch{entityName}";
    }

    public static string QueryListName(string entityName)
    {
        return $"{entityName}ListQuery";
    }

    public static string QueryRecordName(string entityName)
    {
        return $"{entityName}Query";
    }

    public static string CommandAddName(string entityName)
    {
        return $"Add{entityName}Command";
    }

    public static string CommandDeleteName(string entityName)
    {
        return $"Delete{entityName}Command";
    }

    public static string CommandUpdateName(string entityName)
    {
        return $"Update{entityName}Command";
    }

    public static string CommandPatchName(string entityName)
    {
        return $"Patch{entityName}Command";
    }

    public static string FakerName(string objectToFakeName)
    {
        return $"Fake{objectToFakeName}";
    }
    public static string GetDtoName(string entityName, Dto dto)
    {
        return dto switch
        {
            Dto.Manipulation => $"{entityName}ForManipulationDto",
            Dto.Creation => $"{entityName}ForCreationDto",
            Dto.Update => $"{entityName}ForUpdateDto",
            Dto.Read => $"{entityName}Dto",
            Dto.ReadParamaters => $"{entityName}ParametersDto",
            _ => throw new Exception($"Name generator not configured for {Enum.GetName(typeof(Dto), dto)}")
        };
    }

    public static string EndpointBaseGenerator(string entityNamePlural)
    {
        return $@"api/{entityNamePlural.ToLower()}";
    }

    public static string GetBffApiFilenameBase(string entityName, FeatureType type)
    {
        return type.Name switch
        {
            nameof(FeatureType.AddRecord) => $"add{entityName.UppercaseFirstLetter()}",
            nameof(FeatureType.GetList) => $"get{entityName.UppercaseFirstLetter()}List",
            nameof(FeatureType.GetRecord) => $"get{entityName.UppercaseFirstLetter()}",
            nameof(FeatureType.UpdateRecord) => $"update{entityName.UppercaseFirstLetter()}",
            nameof(FeatureType.DeleteRecord) => $"delete{entityName.UppercaseFirstLetter()}",
            _ => throw new Exception($"The '{type.Name}' feature is not supported in bff api scaffolding.")
        };
    }

    public static string ValidatorNameGenerator(string entityName, Validator validator)
    {
        switch (validator)
        {
            case Validator.Manipulation:
                return $"{entityName}ForManipulationDtoValidator";

            case Validator.Creation:
                return $"{entityName}ForCreationDtoValidator";

            case Validator.Update:
                return $"{entityName}ForUpdateDtoValidator";

            default:
                throw new Exception($"Name generator not configured for {Enum.GetName(typeof(Validator), validator)}");
        }
    }
}