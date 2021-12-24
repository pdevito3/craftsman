namespace Craftsman.Helpers
{
    using System;
    using Craftsman.Builders;
    using Craftsman.Builders.Dtos;
    using Craftsman.Builders.Features;
    using Craftsman.Builders.Tests.Fakes;
    using Craftsman.Builders.Tests.FunctionalTests;
    using Craftsman.Builders.Tests.IntegrationTests;
    using Craftsman.Enums;
    using Craftsman.Models;
    using System.Collections.Generic;
    using System.IO;
    using System.IO.Abstractions;
    using System.Linq;
    using Builders.Endpoints;
    using Builders.Tests.Utilities;
    using static Helpers.ConsoleWriter;

    public class EntityScaffolding
    {
        public static void ScaffoldEntities(string srcDirectory,
            string testDirectory,
            string projectBaseName,
            List<Entity> entities,
            string dbContextName,
            bool addSwaggerComments,
            IFileSystem fileSystem)
        {
            foreach (var entity in entities)
            {
                // not worrying about DTOs, profiles, validators, fakers - they are all added by default
                EntityBuilder.CreateEntity(srcDirectory, entity, projectBaseName, fileSystem);
                DtoBuilder.CreateDtos(srcDirectory, entity, projectBaseName);
                ValidatorBuilder.CreateValidators(srcDirectory, projectBaseName, entity);
                ProfileBuilder.CreateProfile(srcDirectory, entity, projectBaseName);
                ApiRouteModifier.AddRoutes(testDirectory, entity, projectBaseName); // api routes always added to testing by default. too much of a pain to scaffold dynamically
                
                var isProtected = entity.Features.Any(f => f.IsProtected); // <-- one more example of why it would be nice to have specific endpoints for each feature 😤
                if(entity.Features.Count > 0)
                    ControllerBuilder.CreateController(srcDirectory, entity.Name, entity.Plural, projectBaseName, isProtected, fileSystem);
                
                // TODO refactor to factory?
                foreach (var feature in entity.Features)
                {
                    AddFeatureToProject(srcDirectory, testDirectory, projectBaseName, dbContextName, addSwaggerComments, feature, entity, feature.IsProtected, fileSystem);
                }

                // Shared Tests
                FakesBuilder.CreateFakes(testDirectory, projectBaseName, entity, fileSystem);
            }
        }

        public static void ScaffoldRolePermissions(string srcDirectory,
            string testDirectory,
            string projectBaseName,
            string dbContextName,
            bool addSwaggerComments,
            IFileSystem fileSystem)
        {
            var rolePermission = new Entity()
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
            
            EntityBuilder.CreateEntity(srcDirectory, rolePermission, projectBaseName, fileSystem);
            DtoBuilder.CreateDtos(srcDirectory, rolePermission, projectBaseName);
            ProfileBuilder.CreateProfile(srcDirectory, rolePermission, projectBaseName);
            ApiRouteModifier.AddRoutes(testDirectory, rolePermission, projectBaseName);
            
            // custom validator
            ValidatorBuilder.CreateRolePermissionValidators(srcDirectory, projectBaseName, rolePermission, fileSystem);
                
            if(rolePermission.Features.Count > 0)
                ControllerBuilder.CreateController(srcDirectory, rolePermission.Name, rolePermission.Plural, projectBaseName, true, fileSystem);
                
            // TODO refactor to factory?
            foreach (var feature in rolePermission.Features)
            {
                AddFeatureToProject(srcDirectory, testDirectory, projectBaseName, dbContextName, addSwaggerComments, feature, rolePermission, true, fileSystem);
            }

            // Shared Tests
            FakesBuilder.CreateRolePermissionFakes(testDirectory, projectBaseName, rolePermission, fileSystem);
            
            // need to do db modifier
            DbContextModifier.AddDbSet(srcDirectory, new List<Entity>() { rolePermission }, dbContextName, projectBaseName);
        }

        public static void AddFeatureToProject(string srcDirectory, string testDirectory, string projectBaseName,
            string dbContextName, bool addSwaggerComments, Feature feature, Entity entity,
            bool isProtected, IFileSystem fileSystem)
        {
            var controllerClassPath = ClassPathHelper.ControllerClassPath(srcDirectory, $"{Utilities.GetControllerName(entity.Plural)}.cs", projectBaseName);
            if (!File.Exists(controllerClassPath.FullClassPath))
                ControllerBuilder.CreateController(srcDirectory, entity.Name, entity.Plural, projectBaseName, isProtected, fileSystem);

            PermissionsModifier.AddPermission(srcDirectory, feature.PermissionName, projectBaseName);
            
            if (feature.Type == FeatureType.AddRecord.Name)
            {
                CommandAddRecordBuilder.CreateCommand(srcDirectory, entity, dbContextName, projectBaseName);
                AddCommandTestBuilder.CreateTests(testDirectory, srcDirectory, entity, projectBaseName);
                CreateEntityTestBuilder.CreateTests(testDirectory, entity, feature.IsProtected, projectBaseName, fileSystem);
                ControllerModifier.AddEndpoint(srcDirectory, FeatureType.AddRecord, entity, addSwaggerComments, 
                    feature, projectBaseName);
            }

            if (feature.Type == FeatureType.GetRecord.Name)
            {
                QueryGetRecordBuilder.CreateQuery(srcDirectory, entity, dbContextName, projectBaseName);
                GetRecordQueryTestBuilder.CreateTests(testDirectory, srcDirectory, entity, projectBaseName);
                GetEntityRecordTestBuilder.CreateTests(testDirectory, entity, feature.IsProtected, projectBaseName, fileSystem);
                ControllerModifier.AddEndpoint(srcDirectory, FeatureType.GetRecord, entity, addSwaggerComments, 
                    feature, projectBaseName);
            }

            if (feature.Type == FeatureType.GetList.Name)
            {
                QueryGetListBuilder.CreateQuery(srcDirectory, entity, dbContextName, projectBaseName);
                GetListQueryTestBuilder.CreateTests(testDirectory, srcDirectory, entity, projectBaseName);
                GetEntityListTestBuilder.CreateTests(testDirectory, entity, feature.IsProtected, projectBaseName, fileSystem);
                ControllerModifier.AddEndpoint(srcDirectory, FeatureType.GetList, entity, addSwaggerComments, 
                    feature, projectBaseName);
            }
            
            if (feature.Type == FeatureType.DeleteRecord.Name)
            {
                CommandDeleteRecordBuilder.CreateCommand(srcDirectory, entity, dbContextName, projectBaseName);
                DeleteCommandTestBuilder.CreateTests(testDirectory, srcDirectory, entity, projectBaseName);
                DeleteEntityTestBuilder.CreateTests(testDirectory, entity, feature.IsProtected, projectBaseName, fileSystem);
                ControllerModifier.AddEndpoint(srcDirectory, FeatureType.DeleteRecord, entity, addSwaggerComments, 
                    feature, projectBaseName);
            }
            
            if (feature.Type == FeatureType.UpdateRecord.Name)
            {
                CommandUpdateRecordBuilder.CreateCommand(srcDirectory, entity, dbContextName, projectBaseName);
                PutCommandTestBuilder.CreateTests(testDirectory, srcDirectory, entity, projectBaseName);
                PutEntityTestBuilder.CreateTests(testDirectory, entity, feature.IsProtected, projectBaseName, fileSystem);
                ControllerModifier.AddEndpoint(srcDirectory, FeatureType.UpdateRecord, entity, addSwaggerComments, 
                    feature, projectBaseName);
            }
            
            if (feature.Type == FeatureType.PatchRecord.Name)
            {
                CommandPatchRecordBuilder.CreateCommand(srcDirectory, entity, dbContextName, projectBaseName);
                PatchCommandTestBuilder.CreateTests(testDirectory, srcDirectory, entity, projectBaseName, fileSystem);
                PatchEntityTestBuilder.CreateTests(testDirectory, entity, feature.IsProtected, projectBaseName, fileSystem);
                ControllerModifier.AddEndpoint(srcDirectory, FeatureType.PatchRecord, entity, addSwaggerComments, 
                    feature, projectBaseName);
            }
            
            if (feature.Type == FeatureType.AddListByFk.Name)
            {
                CommandAddListBuilder.CreateCommand(srcDirectory, entity, dbContextName, projectBaseName, feature, fileSystem);
                AddListCommandTestBuilder.CreateTests(testDirectory, srcDirectory, entity, feature, projectBaseName, fileSystem);
                AddListTestBuilder.CreateTests(testDirectory, entity, feature, projectBaseName, fileSystem);
                ControllerModifier.AddEndpoint(srcDirectory, FeatureType.AddListByFk, entity, addSwaggerComments, 
                    feature, projectBaseName);
            }

            if (feature.Type == FeatureType.AdHoc.Name)
            {
                EmptyFeatureBuilder.CreateCommand(srcDirectory, dbContextName, projectBaseName, feature);
                // TODO ad hoc feature endpoint
                // TODO empty failing test to promote test writing?
            }
        }
    }
}
