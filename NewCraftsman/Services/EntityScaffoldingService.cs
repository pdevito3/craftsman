namespace NewCraftsman.Services
{
    using System.Collections.Generic;
    using System.IO;
    using System.IO.Abstractions;
    using Builders;
    using Builders.Auth;
    using Builders.Bff.Components.Navigation;
    using Builders.Bff.Features.Dynamic;
    using Builders.Bff.Features.Dynamic.Api;
    using Builders.Bff.Features.Dynamic.Types;
    using Builders.Bff.Src;
    using Builders.Dtos;
    using Builders.Endpoints;
    using Builders.Features;
    using Builders.Tests.Fakes;
    using Builders.Tests.FunctionalTests;
    using Builders.Tests.IntegrationTests;
    using Builders.Tests.UnitTests;
    using Builders.Tests.Utilities;
    using Domain;
    using Domain.Enums;
    using Helpers;

    public class EntityScaffoldingService
    {
        private readonly IFileSystem _fileSystem;
        private readonly ICraftsmanUtilities _utilities;

        public EntityScaffoldingService(ICraftsmanUtilities utilities, IFileSystem fileSystem)
        {
            _fileSystem = fileSystem;
            _utilities = utilities;
        }
        
        public void ScaffoldEntities(string solutionDirectory,
            string srcDirectory,
            string testDirectory,
            string projectBaseName,
            List<Entity> entities,
            string dbContextName,
            bool addSwaggerComments,
            bool useSoftDelete)
        {
            foreach (var entity in entities)
            {
                // not worrying about DTOs, profiles, validators, fakers - they are all added by default
                new EntityBuilder(_utilities).CreateEntity(solutionDirectory, srcDirectory, entity, projectBaseName);
                new DtoBuilder(_utilities, _fileSystem).CreateDtos(solutionDirectory, entity, projectBaseName);
                new ValidatorBuilder(_utilities).CreateValidators(solutionDirectory, srcDirectory, projectBaseName, entity);
                new ProfileBuilder(_utilities).CreateProfile(srcDirectory, entity, projectBaseName);
                new ApiRouteModifier(_fileSystem).AddRoutes(testDirectory, entity, projectBaseName); // api routes always added to testing by default. too much of a pain to scaffold dynamically
                
                var isProtected = entity.Features.Any(f => f.IsProtected); // <-- one more example of why it would be nice to have specific endpoints for each feature 😤
                if(entity.Features.Count > 0)
                    new ControllerBuilder(_utilities).CreateController(solutionDirectory, srcDirectory, entity.Name, entity.Plural, projectBaseName, isProtected);
                
                // TODO refactor to factory?
                foreach (var feature in entity.Features)
                    AddFeatureToProject(solutionDirectory, srcDirectory, testDirectory, projectBaseName, dbContextName, addSwaggerComments, feature, entity, useSoftDelete);
                
                // Shared Tests
                new FakesBuilder(_utilities).CreateFakes(solutionDirectory, testDirectory, projectBaseName, entity);
                new CreateEntityUnitTestBuilder(_utilities).CreateTests(solutionDirectory, testDirectory, srcDirectory, entity.Name, entity.Plural, projectBaseName);
                new UpdateEntityUnitTestBuilder(_utilities).CreateTests(solutionDirectory, testDirectory, srcDirectory, entity.Name, entity.Plural, projectBaseName);
            }
        }

        public void ScaffoldRolePermissions(string solutionDirectory, 
            string srcDirectory,
            string testDirectory,
            string projectBaseName,
            string dbContextName,
            bool addSwaggerComments,
            bool useSoftDelete)
        {
            var entity = new Entity()
            {
                Name = "RolePermission",
                Features = new List<Feature>()
                {
                    new() { Type = FeatureType.GetList.Name, IsProtected = true, PermissionName = "CanReadRolePermissions" },
                    new() { Type = FeatureType.GetRecord.Name, IsProtected = true, PermissionName = "CanReadRolePermissions" },
                    new() { Type = FeatureType.AddRecord.Name, IsProtected = true },
                    new() { Type = FeatureType.UpdateRecord.Name, IsProtected = true },
                    new() { Type = FeatureType.DeleteRecord.Name, IsProtected = true },
                },
                Properties = new List<EntityProperty>()
                {
                    new() { Name = "Role", Type = "string", CanFilter = false, CanSort = false },
                    new() { Name = "Permission", Type = "string", CanFilter = false, CanSort = false }
                } 
            };
            
            new EntityBuilder(_utilities).CreateEntity(solutionDirectory, srcDirectory, entity, projectBaseName);
            new DtoBuilder(_utilities, _fileSystem).CreateDtos(solutionDirectory, entity, projectBaseName);
            new ProfileBuilder(_utilities).CreateProfile(srcDirectory, entity, projectBaseName);
            new ApiRouteModifier(_fileSystem).AddRoutes(testDirectory, entity, projectBaseName);
            
            // custom validator
            new ValidatorBuilder(_utilities).CreateRolePermissionValidators(solutionDirectory, srcDirectory, projectBaseName, entity);
                
            if(entity.Features.Count > 0)
                new ControllerBuilder(_utilities).CreateController(solutionDirectory, srcDirectory, entity.Name, entity.Plural, projectBaseName, true);
                
            // TODO refactor to factory?
            foreach (var feature in entity.Features)
            {
                AddFeatureToProject(solutionDirectory, srcDirectory, testDirectory, projectBaseName, dbContextName, addSwaggerComments, feature, entity, useSoftDelete);
            }

            // Shared Tests
            new FakesBuilder(_utilities).CreateRolePermissionFakes(solutionDirectory, testDirectory, projectBaseName, entity);
            new RolePermissionsUnitTestBuilder(_utilities).CreateRolePermissionTests(solutionDirectory, testDirectory, srcDirectory, projectBaseName);
            new RolePermissionsUnitTestBuilder(_utilities).UpdateRolePermissionTests(solutionDirectory, testDirectory, srcDirectory, projectBaseName);
            new UserPolicyHandlerIntegrationTests(_utilities).CreateTests(solutionDirectory, testDirectory, srcDirectory, projectBaseName);
            
            // need to do db modifier
            new DbContextModifier(_fileSystem).AddDbSet(srcDirectory, new List<Entity>() { entity }, dbContextName, projectBaseName);
        }

        public void AddFeatureToProject(string solutionDirectory, string srcDirectory, string testDirectory, string projectBaseName,
            string dbContextName, bool addSwaggerComments, Feature feature, Entity entity, bool useSoftDelete)
        {
            var controllerClassPath = ClassPathHelper.ControllerClassPath(srcDirectory, $"{FileNames.GetControllerName(entity.Plural)}.cs", projectBaseName);
            if (!File.Exists(controllerClassPath.FullClassPath))
                new ControllerBuilder(_utilities).CreateController(solutionDirectory, srcDirectory, entity.Name, entity.Plural, projectBaseName, feature.IsProtected);

            if(feature.IsProtected)
                new PermissionsModifier(_fileSystem).AddPermission(srcDirectory, feature.PermissionName, projectBaseName);
            
            if (feature.Type == FeatureType.AddRecord.Name)
            {
                new CommandAddRecordBuilder(_utilities).CreateCommand(solutionDirectory, srcDirectory, entity, dbContextName, projectBaseName);
                new AddCommandTestBuilder(_utilities).CreateTests(testDirectory, srcDirectory, entity, projectBaseName);
                new CreateEntityTestBuilder(_utilities).CreateTests(solutionDirectory, testDirectory, entity, feature.IsProtected, projectBaseName);
                new ControllerModifier(_fileSystem).AddEndpoint(srcDirectory, FeatureType.AddRecord, entity, addSwaggerComments, 
                    feature, projectBaseName);
            }

            if (feature.Type == FeatureType.GetRecord.Name)
            {
                new QueryGetRecordBuilder(_utilities).CreateQuery(solutionDirectory, srcDirectory, entity, dbContextName, projectBaseName);
                new GetRecordQueryTestBuilder(_utilities).CreateTests(solutionDirectory, testDirectory, srcDirectory, entity, projectBaseName);
                new GetEntityRecordTestBuilder(_utilities).CreateTests(solutionDirectory, testDirectory, entity, feature.IsProtected, projectBaseName);
                new ControllerModifier(_fileSystem).AddEndpoint(srcDirectory, FeatureType.GetRecord, entity, addSwaggerComments, 
                    feature, projectBaseName);
            }

            if (feature.Type == FeatureType.GetList.Name)
            {
                new QueryGetListBuilder(_utilities).CreateQuery(solutionDirectory, srcDirectory, entity, dbContextName, projectBaseName);
                new GetListQueryTestBuilder(_utilities).CreateTests(testDirectory, solutionDirectory, entity, projectBaseName);
                new GetEntityListTestBuilder(_utilities).CreateTests(solutionDirectory, testDirectory, entity, feature.IsProtected, projectBaseName);
                new ControllerModifier(_fileSystem).AddEndpoint(srcDirectory, FeatureType.GetList, entity, addSwaggerComments, 
                    feature, projectBaseName);
            }
            
            if (feature.Type == FeatureType.DeleteRecord.Name)
            {
                new CommandDeleteRecordBuilder(_utilities).CreateCommand(solutionDirectory, srcDirectory, entity, dbContextName, projectBaseName);
                new DeleteCommandTestBuilder(_utilities).CreateTests(solutionDirectory, testDirectory, srcDirectory, entity, projectBaseName, useSoftDelete);
                new DeleteEntityTestBuilder(_utilities).CreateTests(solutionDirectory, testDirectory, entity, feature.IsProtected, projectBaseName);
                new ControllerModifier(_fileSystem).AddEndpoint(srcDirectory, FeatureType.DeleteRecord, entity, addSwaggerComments, 
                    feature, projectBaseName);
            }
            
            if (feature.Type == FeatureType.UpdateRecord.Name)
            {
                new CommandUpdateRecordBuilder(_utilities).CreateCommand(solutionDirectory, srcDirectory, entity, dbContextName, projectBaseName);
                new PutCommandTestBuilder(_utilities).CreateTests(solutionDirectory, testDirectory, srcDirectory, entity, projectBaseName);
                new PutEntityTestBuilder(_utilities).CreateTests(solutionDirectory, testDirectory, entity, feature.IsProtected, projectBaseName);
                new ControllerModifier(_fileSystem).AddEndpoint(srcDirectory, FeatureType.UpdateRecord, entity, addSwaggerComments, 
                    feature, projectBaseName);
            }
            
            if (feature.Type == FeatureType.PatchRecord.Name)
            {
                new CommandPatchRecordBuilder(_utilities).CreateCommand(solutionDirectory, srcDirectory, entity, dbContextName, projectBaseName);
                new PatchCommandTestBuilder(_utilities).CreateTests(solutionDirectory, testDirectory, srcDirectory, entity, projectBaseName);
                new PatchEntityTestBuilder(_utilities).CreateTests(solutionDirectory, testDirectory, entity, feature.IsProtected, projectBaseName);
                new ControllerModifier(_fileSystem).AddEndpoint(srcDirectory, FeatureType.PatchRecord, entity, addSwaggerComments, 
                    feature, projectBaseName);
            }
            
            if (feature.Type == FeatureType.AddListByFk.Name)
            {
                new CommandAddListBuilder(_utilities).CreateCommand(solutionDirectory, srcDirectory, entity, dbContextName, projectBaseName, feature);
                new AddListCommandTestBuilder(_utilities).CreateTests(solutionDirectory, testDirectory, srcDirectory, entity, feature, projectBaseName);
                new AddListTestBuilder(_utilities).CreateTests(solutionDirectory, testDirectory, entity, feature, projectBaseName);
                new ControllerModifier(_fileSystem).AddEndpoint(srcDirectory, FeatureType.AddListByFk, entity, addSwaggerComments, 
                    feature, projectBaseName);
            }

            if (feature.Type == FeatureType.AdHoc.Name)
            {
                new EmptyFeatureBuilder(_utilities).CreateCommand(srcDirectory, dbContextName, projectBaseName, feature);
                // TODO ad hoc feature endpoint
                // TODO empty failing test to promote test writing?
            }
        }
        
        public void ScaffoldBffEntities(List<BffEntity> entities, string spaDirectory)
        {
            foreach (var templateEntity in entities)
            {
                new DynamicFeatureBuilder(_utilities).CreateDynamicFeatureIndex(spaDirectory, templateEntity.Plural);
                new DynamicFeatureRoutesBuilder(_utilities).CreateDynamicFeatureRoutes(spaDirectory, templateEntity.Name,
                    templateEntity.Plural);
                new DynamicFeatureTypesBuilder(_utilities).CreateDynamicFeatureTypes(spaDirectory, templateEntity.Name, templateEntity.Plural,
                    templateEntity.Properties);
                new NavigationComponentModifier(_fileSystem).AddFeatureListRouteToNav(spaDirectory, templateEntity.Plural);
                new DynamicFeatureRoutesModifier(_fileSystem).AddRoute(spaDirectory, templateEntity.Name, templateEntity.Plural);

                // apis
                new DynamicFeatureKeysBuilder(_utilities).CreateDynamicFeatureKeys(spaDirectory, templateEntity.Name, templateEntity.Plural);
                new DynamicFeatureApiIndexBuilder(_utilities).CreateDynamicFeatureApiIndex(spaDirectory, templateEntity.Name,
                    templateEntity.Plural);
                foreach (var templateEntityFeature in templateEntity.Features)
                {
                    new DynamicFeatureApiIndexModifier(_fileSystem).AddFeature(spaDirectory, templateEntity.Name, templateEntity.Plural,
                        FeatureType.FromName(templateEntityFeature.Type));

                    if (templateEntityFeature.Type == FeatureType.AddRecord.Name)
                        new DynamicFeatureAddEntityBuilder(_utilities).CreateApiFile(spaDirectory, templateEntity.Name,
                            templateEntity.Plural);
                    if (templateEntityFeature.Type == FeatureType.GetList.Name)
                        new DynamicFeatureGetListEntityBuilder(_utilities).CreateApiFile(spaDirectory, templateEntity.Name,
                            templateEntity.Plural);
                    if (templateEntityFeature.Type == FeatureType.DeleteRecord.Name)
                        new DynamicFeatureDeleteEntityBuilder(_utilities).CreateApiFile(spaDirectory, templateEntity.Name,
                            templateEntity.Plural);
                    if (templateEntityFeature.Type == FeatureType.UpdateRecord.Name)
                        new DynamicFeatureUpdateEntityBuilder(_utilities).CreateApiFile(spaDirectory, templateEntity.Name,
                            templateEntity.Plural);
                    if (templateEntityFeature.Type == FeatureType.GetRecord.Name)
                        new DynamicFeatureGetEntityBuilder(_utilities).CreateApiFile(spaDirectory, templateEntity.Name, templateEntity.Plural);
                }
            }
        }
    }
}
