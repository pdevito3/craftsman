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

    public static string UserRolesUpdateDomainMessage() => "UserRolesUpdated";

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

    public static string CreateEntityUnitTestName(string entityName) => $"Create{entityName}Tests";

    public static string GetEntityListUnitTestName(string entityName) => $"Get{entityName}ListTests";

    public static string UpdateEntityUnitTestName(string entityName) => $"Update{entityName}Tests";

    public static string GetEntityFeatureClassName(string entityName) => $"Get{entityName}";

    public static string GetEntityListFeatureClassName(string entityName) => $"Get{entityName}List";

    public static string GetEntityFormViewFeatureClassName(string entityName) => $"Get{entityName}FormView";

    public static string GetEntityListViewFeatureClassName(string entityName) => $"Get{entityName}ListView";

    public static string AddEntityFeatureClassName(string entityName) => $"Add{entityName}";

    public static string DeleteEntityFeatureClassName(string entityName) => $"Delete{entityName}";

    public static string UpdateEntityFeatureClassName(string entityName) => $"Update{entityName}";
    public static string PatchEntityFeatureClassName(string entityName) =>  $"Patch{entityName}";
    public static string AddUserRoleFeatureClassName() => $"AddUserRole";
    public static string RemoveUserRoleFeatureClassName() => $"RemoveUserRole";

    public static string QueryListName() => $"Query";
    public static string QueryRecordName() => $"Query";
    public static string QueryFormViewName() => $"Query";
    public static string QueryListViewName() => $"Query";
    public static string CommandAddName() => $"Command";
    public static string CommandDeleteName() => $"Command";
    public static string CommandUpdateName() => $"Command";
    public static string CommandPatchName() => $"Command";
    public static string FakerName(string objectToFakeName) => $"Fake{objectToFakeName}";    
    public static string UnitTestUtilsName() => $"UnitTestUtils";
    
    public static string GetDtoName(string entityName, Dto dto)
    {
        return dto switch
        {
            Dto.Manipulation => $"{entityName}ForManipulationDto",
            Dto.Creation => $"{entityName}ForCreationDto",
            Dto.Update => $"{entityName}ForUpdateDto",
            Dto.Read => $"{entityName}Dto",
            Dto.ReadParameters => $"{entityName}ParametersDto",
            Dto.FormView => $"{entityName}FormViewDto",
            Dto.ListView => $"{entityName}ListViewDto",
            Dto.ListViewParameters => $"{entityName}ListViewParametersDto",
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