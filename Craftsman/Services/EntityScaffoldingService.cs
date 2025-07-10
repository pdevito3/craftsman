namespace Craftsman.Services;

using System.Collections.Generic;
using System.IO;
using System.IO.Abstractions;
using Builders;
using Builders.Auth;
using Builders.Dtos;
using Builders.Endpoints;
using Builders.EntityModels;
using Builders.Features;
using Builders.Tests.Fakes;
using Builders.Tests.FunctionalTests;
using Builders.Tests.IntegrationTests;
using Builders.Tests.IntegrationTests.UserRoles;
using Builders.Tests.UnitTests;
using Builders.Tests.Utilities;
using Domain;
using Domain.Enums;
using Helpers;
using MediatR;

public class EntityScaffoldingService(ICraftsmanUtilities utilities, IFileSystem fileSystem, IMediator mediator, IConsoleWriter consoleWriter)
{
    public void ScaffoldEntities(string solutionDirectory,
        string srcDirectory,
        string testDirectory,
        string projectBaseName,
        List<Entity> entities,
        string dbContextName,
        bool addSwaggerComments,
        bool useSoftDelete,
        DbProvider dbProvider)
    {
        foreach (var entity in entities)
        {
            // not worrying about DTOs, profiles, validators, fakers - they are all added by default
            mediator.Send(new EntityBuilder.CreateEntityCommand(solutionDirectory, entity));
            mediator.Send(new DtoBuilder.CreateDtosCommand(entity));
            mediator.Send(new EntityModelBuilder.EntityModelBuilderCommand(entity));
            mediator.Send(new EntityMappingBuilder.EntityMappingBuilderCommand(entity.Name, entity.Plural));
            new ApiRouteModifier(fileSystem, consoleWriter).AddRoutes(testDirectory, entity, projectBaseName); // api routes always added to testing by default. too much of a pain to scaffold dynamically

            mediator.Send(new DatabaseEntityConfigBuilder.Command(entity.Name, entity.Plural, entity.Properties));

            var isProtected = entity.Features.Any(f => f.IsProtected); // <-- one more example of why it would be nice to have specific endpoints for each feature 😤
            if (entity.Features.Count > 0)
                mediator.Send(new ControllerBuilder.ControllerBuilderCommand(entity.Plural, projectBaseName, isProtected));

            // TODO refactor to factory?
            foreach (var feature in entity.Features)
                AddFeatureToProject(solutionDirectory, srcDirectory, testDirectory, projectBaseName, dbContextName, addSwaggerComments, feature, entity, useSoftDelete);

            // Shared Tests
            mediator.Send(new FakesBuilder.CreateFakesCommand(entity)).GetAwaiter().GetResult();
            mediator.Send(new FakeEntityBuilderBuilder.Command(entity)).GetAwaiter().GetResult();
            mediator.Send(new CreateEntityUnitTestBuilder.Command(entity.Name, entity.Plural, entity.Properties)).GetAwaiter().GetResult();
            mediator.Send(new UpdateEntityUnitTestBuilder.Command(entity.Name, entity.Plural, entity.Properties)).GetAwaiter().GetResult();

            // domain events
            mediator.Send(new CreatedDomainEventBuilder.CreatedDomainEventBuilderCommand(entity.Name, entity.Plural));
            mediator.Send(new UpdatedDomainEventBuilder.UpdatedDomainEventBuilderCommand(entity.Name, entity.Plural));
        }

        AddRelationshipsAsync(srcDirectory, projectBaseName, entities).Wait();
        AddStringArrayItemsAsync(srcDirectory, projectBaseName, entities, dbProvider).Wait();
        AddValueObjects(srcDirectory, projectBaseName, entities);

        new DbContextModifier(fileSystem).AddDbSetAndConfig(srcDirectory, entities, dbContextName, projectBaseName);
    }

    private async Task AddRelationshipsAsync(string srcDirectory, string projectBaseName, List<Entity> entities)
    {
        // reloop once all bases are added for relationships to mod on top -- could push into the earlier loop if perf becomes an issue
        foreach (var entity in entities)
        {
            var allPropsNotNone = entity.Properties.Where(x => !x.GetDbRelationship.IsNone).ToList();
            foreach (var entityProperty in allPropsNotNone)
            {
                await entityProperty.GetDbRelationship.UpdateEntityPropertiesAsync(mediator, 
                    srcDirectory,
                    entity.Name,
                    entity.Plural,
                    entityProperty.ForeignEntityName,
                    entityProperty.ForeignEntityPlural,
                    entityProperty.Name,
                    projectBaseName);
                
                await entityProperty.GetDbRelationship.UpdateEntityManagementMethodsAsync(mediator, 
                    srcDirectory,
                    entity.Name,
                    entity.Plural,
                    entityProperty,
                    projectBaseName);
                
                await mediator.Send(new EntityModifier.AddParentRelationshipEntityCommand(
                    entityProperty, 
                    entity.Name, 
                    entity.Plural));
                
                new DatabaseEntityConfigModifier(fileSystem, consoleWriter).AddRelationships(srcDirectory, 
                    entity.Name,
                    entity.Plural, 
                    entityProperty, 
                    projectBaseName);
            }
        }
    }

    public void AddValueObjects(string srcDirectory, string projectBaseName, List<Entity> entities)
    {
        foreach (var entity in entities)
        {
            var valueObjectProps = entity.Properties.Where(x => x.IsValueObject).ToList();
            foreach (var valueObjectProp in valueObjectProps)
            {
                new DatabaseEntityConfigModifier(fileSystem, consoleWriter).AddValueObjectConfig(srcDirectory, 
                    entity.Name,
                    valueObjectProp, 
                    projectBaseName);
            }
        }
        
        var baseValueObjects = new List<EntityProperty>
        {
            new()
            {
                ValueObjectName = "Email",
                AsValueObject = "Email",
            },
            new()
            {
                Name = "Percent",
                ValueObjectName = "Percent",
                ValueObjectPlural = "Percentages",
                AsValueObject = "Percent",
            },
            new()
            {
                Name = "MonetaryAmount",
                ValueObjectName = "MonetaryAmount",
                AsValueObject = "MonetaryAmount",
            }
        };

        var distinctValueObjects = entities
            .SelectMany(x => x.Properties.Where(p => p.IsValueObject))
            .ToList();
        distinctValueObjects.AddRange(baseValueObjects);
        distinctValueObjects = distinctValueObjects.DistinctBy(x => x.ValueObjectName).ToList();
        foreach (var valueObjectProp in distinctValueObjects)
        {
            var classPath = ClassPathHelper.WebApiValueObjectsClassPath(srcDirectory, 
                $"{valueObjectProp.ValueObjectName}.cs",
                valueObjectProp.ValueObjectPlural,
                projectBaseName);
            
            if (fileSystem.File.Exists(classPath.FullClassPath))
                continue;
            
            var fileText = valueObjectProp.ValueObjectType.GetFileText(classPath.ClassNamespace, 
                valueObjectProp.ValueObjectName,
                valueObjectProp.Type,
                valueObjectProp.SmartNames,
                srcDirectory,
                projectBaseName);
            utilities.CreateFile(classPath, fileText);
        }
        
        var entitiesThatHaveValueObjectProperties = entities
            .Where(x => x.Properties.Any(p => p.IsValueObject))
            .ToList();
        foreach (var entityThatHasValueObjectProperties in entitiesThatHaveValueObjectProperties)
        {
            var voProperties = entityThatHasValueObjectProperties.Properties.Where(x => x.IsValueObject).ToList();
            foreach (var entityProperty in voProperties)
            {
                if (entityProperty.Type.ToLowerInvariant() != "string")
                {
                    new EntityMappingModifier(fileSystem, consoleWriter)
                        .UpdateMappingAttributesForValueObject(srcDirectory, 
                            entityThatHasValueObjectProperties.Name,
                            entityThatHasValueObjectProperties.Plural, 
                            entityProperty, 
                            projectBaseName);
                }
            }
        }
    }

    public async Task AddStringArrayItemsAsync(string srcDirectory, string projectBaseName, List<Entity> entities, DbProvider dbProvider)
    {
        foreach (var entity in entities)
        {
            var stringArrayProps = entity.Properties.Where(x => x.IsStringArray).ToList();
            foreach (var stringArrayProp in stringArrayProps)
            {
                await mediator.Send(new EntityModifier.AddStringArrayManagementCommand(
                    stringArrayProp,
                    entity.Name,
                    entity.Plural));
                
                new DatabaseEntityConfigModifier(fileSystem, consoleWriter).AddStringArrayProperty(srcDirectory, 
                    entity.Name,
                    stringArrayProp, 
                    dbProvider,
                    projectBaseName);
            }
            
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
                    new() { Type = FeatureType.AddRecord.Name, IsProtected = true, PermissionName = "CanAddPermissions" },
                    new() { Type = FeatureType.UpdateRecord.Name, IsProtected = true, PermissionName = "CanUpdatePermissions" },
                    new() { Type = FeatureType.DeleteRecord.Name, IsProtected = true, PermissionName = "CanDeletePermissions" }
                },
            Properties = new List<EntityProperty>()
                {
                    new() { Name = "Role", Type = "string" },
                    new() { Name = "Permission", Type = "string" }
                }
        };

        mediator.Send(new EntityBuilder.CreateRolePermissionsEntityCommand(entity));
        mediator.Send(new DtoBuilder.CreateDtosCommand(entity));
        mediator.Send(new EntityModelBuilder.EntityModelBuilderCommand(entity));
        mediator.Send(new EntityMappingBuilder.EntityMappingBuilderCommand(entity.Name, entity.Plural));
        new ApiRouteModifier(fileSystem, consoleWriter).AddRoutes(testDirectory, entity, projectBaseName);
        mediator.Send(new DatabaseEntityConfigRolePermissionBuilder.Command());

        if (entity.Features.Count > 0)
            mediator.Send(new ControllerBuilder.ControllerBuilderCommand(entity.Plural, projectBaseName, true));

        // TODO refactor to factory?
        foreach (var feature in entity.Features)
        {
            AddFeatureToProject(solutionDirectory, srcDirectory, testDirectory, projectBaseName, dbContextName, addSwaggerComments, feature, entity, useSoftDelete);
        }

        // Shared Tests
        mediator.Send(new FakesBuilder.CreateRolePermissionFakesCommand(entity)).GetAwaiter().GetResult();
        mediator.Send(new RolePermissionsUnitTestBuilder.CreateRolePermissionTestsCommand()).GetAwaiter().GetResult();
        mediator.Send(new RolePermissionsUnitTestBuilder.UpdateRolePermissionTestsCommand()).GetAwaiter().GetResult();
        mediator.Send(new FakeEntityBuilderBuilder.Command(entity)).GetAwaiter().GetResult();
        
        // need to do db modifier
        new DbContextModifier(fileSystem).AddDbSetAndConfig(srcDirectory, new List<Entity>() { entity }, dbContextName, projectBaseName);

        // domain events
        mediator.Send(new CreatedDomainEventBuilder.CreatedDomainEventBuilderCommand(entity.Name, entity.Plural));
        mediator.Send(new UpdatedDomainEventBuilder.UpdatedDomainEventBuilderCommand(entity.Name, entity.Plural));
    }


    public void ScaffoldUser(string solutionDirectory,
        string srcDirectory,
        string testDirectory,
        string projectBaseName,
        string dbContextName,
        bool addSwaggerComments,
        bool useSoftDelete)
    {
        var userEntity = new Entity()
        {
            Name = "User",
            Features =
            [
                new() { Type = FeatureType.GetList.Name, IsProtected = true, PermissionName = "CanGetUsers" },
                new() { Type = FeatureType.GetRecord.Name, IsProtected = true, PermissionName = "CanGetUsers" },
                new() { Type = FeatureType.AddRecord.Name, IsProtected = true, PermissionName = "CanAddUsers" },
                new() { Type = FeatureType.UpdateRecord.Name, IsProtected = true, PermissionName = "CanUpdateUsers" },
                new() { Type = FeatureType.DeleteRecord.Name, IsProtected = true, PermissionName = "CanDeleteUsers" }
            ],
            Properties =
            [
                new() { Name = "Identifier", Type = "string" },
                new() { Name = "FirstName", Type = "string" },
                new() { Name = "LastName", Type = "string" },
                new() { Name = "Email", Type = "string" },
                new() { Name = "Username", Type = "string" },
                new() { Name = "UserRoles", Type = "ICollection<UserRole>", ForeignEntityPlural = "UserRoles" }
            ]
        };

        mediator.Send(new EntityBuilder.CreateUserEntityCommand(userEntity));
        mediator.Send(new EntityBuilder.CreateUserRoleEntityCommand());
        
        // TODO custom dto for roles
        mediator.Send(new DtoBuilder.CreateDtosCommand(userEntity));
        
        mediator.Send(new EntityModelBuilder.EntityModelBuilderCommand(userEntity));
        mediator.Send(new EntityMappingBuilder.EntityMappingBuilderCommand("User", "Users"));
        new ApiRouteModifier(fileSystem, consoleWriter).AddRoutesForUser(testDirectory, projectBaseName);
        mediator.Send(new DatabaseEntityConfigUserBuilder.Command());
        mediator.Send(new DatabaseEntityConfigUserRoleBuilder.Command());
        
        mediator.Send(new ControllerBuilder.ControllerBuilderCommand(userEntity.Plural, projectBaseName, true));
        new ControllerModifier(fileSystem).AddCustomUserEndpoint(srcDirectory, projectBaseName);
        
        foreach (var feature in userEntity.Features)
        {
            AddFeatureToProject(solutionDirectory, srcDirectory, testDirectory, projectBaseName, dbContextName, addSwaggerComments, feature, userEntity, useSoftDelete);
        }
        mediator.Send(new CommandAddUserRoleBuilder.CommandAddUserRoleBuilderCommand(userEntity, dbContextName));
        mediator.Send(new CommandRemoveUserRoleBuilder.CommandRemoveUserRoleBuilderCommand(userEntity, dbContextName));
        // new AddUserFeatureBuilder(_utilities).AddFeature(srcDirectory, projectBaseName);
        new AddUserFeatureOverrideModifier(fileSystem).UpdateAddUserFeature(srcDirectory, projectBaseName, dbContextName);

        // extra testing
        mediator.Send(new FakesBuilder.CreateUserFakesCommand(userEntity)).GetAwaiter().GetResult();
        mediator.Send(new CreateUserRoleUnitTestBuilder.Command()).GetAwaiter().GetResult();
        mediator.Send(new AddRemoveUserRoleTestsBuilder.Command()).GetAwaiter().GetResult();
        mediator.Send(new UserUnitTestBuilder.CreateTestsCommand()).GetAwaiter().GetResult();
        mediator.Send(new UserUnitTestBuilder.UpdateTestsCommand()).GetAwaiter().GetResult();
        mediator.Send(new FakeEntityBuilderBuilder.Command(userEntity)).GetAwaiter().GetResult();
        
        // need to do db modifier
        new DbContextModifier(fileSystem).AddDbSetAndConfig(srcDirectory, [userEntity], dbContextName, projectBaseName);
        new DbContextModifier(fileSystem).AddDbSetAndConfig(srcDirectory, [
            new Entity() { Name = "UserRole", Plural = "UserRoles" }
        ], dbContextName, projectBaseName);

        // domain events
        mediator.Send(new CreatedDomainEventBuilder.CreatedDomainEventBuilderCommand(userEntity.Name, userEntity.Plural));
        mediator.Send(new UpdatedDomainEventBuilder.UpdatedDomainEventBuilderCommand(userEntity.Name, userEntity.Plural));
        mediator.Send(new UpdatedUserRoleDomainEventBuilder.Command());
    }

    public void AddFeatureToProject(string solutionDirectory, string srcDirectory, string testDirectory, string projectBaseName,
        string dbContextName, bool addSwaggerComments, Feature feature, Entity entity, bool useSoftDelete)
    {
        var controllerClassPath = ClassPathHelper.ControllerClassPath(srcDirectory, $"{FileNames.GetControllerName(entity.Plural)}.cs", projectBaseName);
        if (!File.Exists(controllerClassPath.FullClassPath))
            mediator.Send(new ControllerBuilder.ControllerBuilderCommand(entity.Plural, projectBaseName, feature.IsProtected));

        if (feature.IsProtected)
            new PermissionsModifier(fileSystem).AddPermission(srcDirectory, feature.PermissionName, projectBaseName);

        if (feature.Type == FeatureType.AddRecord.Name)
        {
            mediator.Send(new CommandAddRecordBuilder.CommandAddRecordBuilderCommand(entity, feature.IsProtected, feature.PermissionName, dbContextName));
            switch (entity.Name)
            {
                case "RolePermission":
                    mediator.Send(new Craftsman.Builders.Tests.IntegrationTests.RolePermissions.AddCommandTestBuilder.CreateTestsCommand(testDirectory, srcDirectory, entity, projectBaseName)).GetAwaiter().GetResult();
                    break;
                case "User":
                    mediator.Send(new Craftsman.Builders.Tests.IntegrationTests.Users.AddCommandTestBuilder.CreateTestsCommand(testDirectory, srcDirectory, entity, projectBaseName)).GetAwaiter().GetResult();
                    break;
                default:
                    mediator.Send(new AddCommandTestBuilder.Command(entity, feature.PermissionName, feature.IsProtected)).GetAwaiter().GetResult();
                    break;
            }

            new ControllerModifier(fileSystem).AddEndpoint(srcDirectory, FeatureType.AddRecord, entity, addSwaggerComments,
                feature, projectBaseName);
        }

        if (feature.Type == FeatureType.GetRecord.Name)
        {
            mediator.Send(new QueryGetRecordBuilder.Command(entity, feature.IsProtected, feature.PermissionName, dbContextName));
            switch (entity.Name)
            {
                case "RolePermission":
                    mediator.Send(new Craftsman.Builders.Tests.IntegrationTests.RolePermissions.GetRecordQueryTestBuilder.CreateTestsCommand(solutionDirectory, testDirectory, srcDirectory, entity, projectBaseName)).GetAwaiter().GetResult();
                    break;
                case "User":
                    mediator.Send(new Craftsman.Builders.Tests.IntegrationTests.Users.GetRecordQueryTestBuilder.CreateTestsCommand(solutionDirectory, testDirectory, srcDirectory, entity, projectBaseName)).GetAwaiter().GetResult();
                    break;
                default:
                    mediator.Send(new GetRecordQueryTestBuilder.Command(entity, feature.PermissionName, feature.IsProtected)).GetAwaiter().GetResult();
                    break;
            }

            new ControllerModifier(fileSystem).AddEndpoint(srcDirectory, FeatureType.GetRecord, entity, addSwaggerComments,
                feature, projectBaseName);
        }

        if (feature.Type == FeatureType.GetList.Name)
        {
            mediator.Send(new QueryGetListBuilder.Command(entity, feature.IsProtected, feature.PermissionName, dbContextName));
            mediator.Send(new GetListQueryTestBuilder.Command(entity, feature.PermissionName, feature.IsProtected)).GetAwaiter().GetResult();
            new ControllerModifier(fileSystem).AddEndpoint(srcDirectory, FeatureType.GetList, entity, addSwaggerComments,
                feature, projectBaseName);
        }

        if (feature.Type == FeatureType.GetAll.Name)
        {
            mediator.Send(new QueryGetAllBuilder.Command(entity, feature.IsProtected, feature.PermissionName, dbContextName));
            mediator.Send(new GetAllQueryTestBuilder.Command(entity, feature.PermissionName, feature.IsProtected)).GetAwaiter().GetResult();
            new ControllerModifier(fileSystem).AddEndpoint(srcDirectory, FeatureType.GetAll, entity, addSwaggerComments,
                feature, projectBaseName);
        }

        if (feature.Type == FeatureType.DeleteRecord.Name)
        {
            mediator.Send(new CommandDeleteRecordBuilder.CommandDeleteRecordBuilderCommand(entity, feature.IsProtected, feature.PermissionName, dbContextName));
            mediator.Send(new DeleteCommandTestBuilder.Command(entity, useSoftDelete, feature.PermissionName, feature.IsProtected)).GetAwaiter().GetResult();
            new ControllerModifier(fileSystem).AddEndpoint(srcDirectory, FeatureType.DeleteRecord, entity, addSwaggerComments,
                feature, projectBaseName);
        }

        if (feature.Type == FeatureType.UpdateRecord.Name)
        {
            mediator.Send(new CommandUpdateRecordBuilder.Command(entity, feature.IsProtected, feature.PermissionName, dbContextName));
            
            switch (entity.Name)
            {
                case "RolePermission":
                    mediator.Send(new Craftsman.Builders.Tests.IntegrationTests.RolePermissions.PutCommandTestBuilder.CreateTestsCommand(solutionDirectory, testDirectory, srcDirectory, entity, projectBaseName)).GetAwaiter().GetResult();
                    break;
                case "User":
                    mediator.Send(new Craftsman.Builders.Tests.IntegrationTests.Users.PutCommandTestBuilder.CreateTestsCommand(solutionDirectory, testDirectory, srcDirectory, entity, projectBaseName)).GetAwaiter().GetResult();
                    break;
                default:
                    mediator.Send(new PutCommandTestBuilder.Command(entity, feature.IsProtected, feature.PermissionName)).GetAwaiter().GetResult();
                    break;
            }
            
            new ControllerModifier(fileSystem).AddEndpoint(srcDirectory, FeatureType.UpdateRecord, entity, addSwaggerComments,
                feature, projectBaseName);
        }

        // if (feature.Type == FeatureType.PatchRecord.Name)
        // {
        //     new CommandPatchRecordBuilder(_utilities).CreateCommand(srcDirectory, entity, projectBaseName, feature.IsProtected, feature.PermissionName);
        //     new PatchCommandTestBuilder(_utilities).CreateTests(solutionDirectory, testDirectory, srcDirectory, entity, projectBaseName);
        //     new PatchEntityTestBuilder(_utilities).CreateTests(solutionDirectory, srcDirectory, testDirectory, entity, feature.IsProtected, projectBaseName);
        //     new ControllerModifier(_fileSystem).AddEndpoint(srcDirectory, FeatureType.PatchRecord, entity, addSwaggerComments,
        //         feature, projectBaseName);
        // }

        if (feature.Type == FeatureType.AddListByFk.Name)
        {
            mediator.Send(new CommandAddListBuilder.Command(entity, feature, feature.IsProtected, feature.PermissionName, dbContextName)).GetAwaiter().GetResult();
            mediator.Send(new AddListCommandTestBuilder.Command(entity, feature, feature.PermissionName, feature.IsProtected)).GetAwaiter().GetResult();
            new ControllerModifier(fileSystem).AddEndpoint(srcDirectory, FeatureType.AddListByFk, entity, addSwaggerComments,
                feature, projectBaseName);
        }

        if (feature.Type == FeatureType.AdHoc.Name)
        {
            mediator.Send(new EmptyFeatureBuilder.Command(dbContextName, feature)).GetAwaiter().GetResult();
            // TODO ad hoc feature endpoint
            // TODO empty failing test to promote test writing?
        }

        if (feature.Type == FeatureType.Job.Name)
        {
            mediator.Send(new JobFeatureBuilder.Command(feature, entity.Plural, dbContextName));
            mediator.Send(new JobFeatureIntegrationTestBuilder.Command(feature, entity.Plural, dbContextName));
        }
    }
}
