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

    public static string FakeBuilderName(string entityName) => $"Fake{entityName}Builder";    
    public static string MessageInterfaceName(string messageName) => $"I{messageName}";
    public static string EntityCreatedDomainMessage(string entityName) => $"{entityName}Created";    
    public static string EntityUpdatedDomainMessage(string entityName) => $"{entityName}Updated";    
    public static string UserRolesUpdateDomainMessage() => "UserRolesUpdated";
    public static string GetApiRouteClass(string entityPlural) => entityPlural;    
    public static string GetWebHostFactoryName() => "TestingWebApplicationFactory";    
    public static string GetFunctionalFixtureName() => "FunctionalTestFixture";    
    public static string GetControllerName(string entityName) => $"{entityName}Controller";    
    public static string GetDatabaseEntityConfigName(string entityName) => $"{entityName}Configuration";    
    public static string GetDatabaseHelperFileName() => $"DatabaseHelper";    
    public static string GetSeederName(Entity entity) => $"{entity.Name}Seeder";
    public static string GetInfraRegistrationName() => "InfrastructureServiceExtension";
    public static string GetSwaggerServiceExtensionName() => "SwaggerServiceExtension";
    public static string GetAppSettingsName(bool asJson = true) => asJson ? $"appsettings.json" : $"appsettings";
    public static string BffApiKeysFilename(string entityName) => $"{entityName.LowercaseFirstLetter()}.keys";
    public static string BffEntityListRouteComponentName(string entityName) => $"{entityName.UppercaseFirstLetter()}List";
    public static string BffApiKeysExport(string entityName) => $"{entityName.UppercaseFirstLetter()}Keys";
    public static string NextJsApiKeysFilename(string entityName) => $"{entityName.LowercaseFirstLetter()}.keys";
    public static string NextJsEntityListRouteComponentName(string entityName) => $"{entityName.UppercaseFirstLetter()}List";
    
    public static string NextJsEntityFeatureListTableName(string entityName) => $"{entityName}ListTable";

    public static string NextJsEntityFeatureFormName(string entityName) => $"{entityName}Form";
    public static string NextJsEntityValidationName(string entityName) => $"{entityName.LowercaseFirstLetter()}ValidationSchema";
    public static string NextJsApiKeysExport(string entityName) => $"{entityName.UppercaseFirstLetter()}Keys";
    public static string GetMappingName(string entityName) => $"{entityName}Mappings";
    public static string GetIntegrationTestFixtureName() => $"TestFixture";

    public static string CreateEntityUnitTestName(string entityName) => $"Create{entityName}Tests";

    public static string GetEntityListUnitTestName(string entityName) => $"Get{entityName}ListTests";

    public static string UpdateEntityUnitTestName(string entityName) => $"Update{entityName}Tests";

    public static string GetEntityFeatureClassName(string entityName) => $"Get{entityName}";

    public static string GetEntityListFeatureClassName(string entityName) => $"Get{entityName}List";

    public static string AddEntityFeatureClassName(string entityName) => $"Add{entityName}";

    public static string DeleteEntityFeatureClassName(string entityName) => $"Delete{entityName}";

    public static string UpdateEntityFeatureClassName(string entityName) => $"Update{entityName}";
    public static string PatchEntityFeatureClassName(string entityName) =>  $"Patch{entityName}";
    public static string AddUserRoleFeatureClassName() => $"AddUserRole";
    public static string RemoveUserRoleFeatureClassName() => $"RemoveUserRole";

    public static string QueryListName()
    {
        return $"Query";
    }

    public static string QueryRecordName()
    {
        return $"Query";
    }

    public static string CommandAddName()
    {
        return $"Command";
    }

    public static string CommandDeleteName()
    {
        return $"Command";
    }

    public static string CommandUpdateName()
    {
        return $"Command";
    }

    public static string CommandPatchName()
    {
        return $"Command";
    }

    public static string FakerName(string objectToFakeName)
    {
        return $"Fake{objectToFakeName}";
    }
    
    public static string UnitTestUtilsName()
    {
        return $"UnitTestUtils";
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
}