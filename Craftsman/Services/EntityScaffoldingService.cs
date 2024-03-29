﻿namespace Craftsman.Services;

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
    private readonly IFileSystem _fileSystem = fileSystem;
    private readonly ICraftsmanUtilities _utilities = utilities;
    private readonly IMediator _mediator = mediator;
    private readonly IConsoleWriter _consoleWriter = consoleWriter;

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
            new EntityBuilder(_utilities).CreateEntity(solutionDirectory, srcDirectory, entity, projectBaseName);
            new DtoBuilder(_utilities, _fileSystem).CreateDtos(srcDirectory, entity, projectBaseName);
            new EntityModelBuilder(_utilities, _fileSystem).CreateEntityModels(srcDirectory, entity, projectBaseName);
            new EntityMappingBuilder(_utilities).CreateMapping(srcDirectory, entity.Name, entity.Plural, projectBaseName);
            new ApiRouteModifier(_fileSystem, _consoleWriter).AddRoutes(testDirectory, entity, projectBaseName); // api routes always added to testing by default. too much of a pain to scaffold dynamically

            _mediator.Send(new DatabaseEntityConfigBuilder.Command(entity.Name, entity.Plural, entity.Properties));
            _mediator.Send(new EntityRepositoryBuilder.Command(dbContextName, 
                entity.Name, 
                entity.Plural));

            var isProtected = entity.Features.Any(f => f.IsProtected); // <-- one more example of why it would be nice to have specific endpoints for each feature 😤
            if (entity.Features.Count > 0)
                new ControllerBuilder(_utilities).CreateController(solutionDirectory, srcDirectory, entity.Plural, projectBaseName, isProtected);

            // TODO refactor to factory?
            foreach (var feature in entity.Features)
                AddFeatureToProject(solutionDirectory, srcDirectory, testDirectory, projectBaseName, dbContextName, addSwaggerComments, feature, entity, useSoftDelete);

            // Shared Tests
            new FakesBuilder(_utilities).CreateFakes(srcDirectory, testDirectory, projectBaseName, entity);
            new FakeEntityBuilderBuilder(_utilities).CreateFakeBuilder(srcDirectory, testDirectory, projectBaseName, entity);
            new CreateEntityUnitTestBuilder(_utilities)
                .CreateTests(solutionDirectory, testDirectory, srcDirectory, entity.Name, entity.Plural, entity.Properties, projectBaseName);
            new UpdateEntityUnitTestBuilder(_utilities)
                .CreateTests(solutionDirectory, testDirectory, srcDirectory, entity.Name, entity.Plural, entity.Properties, projectBaseName);

            // domain events
            _mediator.Send(new CreatedDomainEventBuilder.CreatedDomainEventBuilderCommand(entity.Name, entity.Plural));
            _mediator.Send(new UpdatedDomainEventBuilder.UpdatedDomainEventBuilderCommand(entity.Name, entity.Plural));
        }

        AddRelationships(srcDirectory, projectBaseName, entities);
        AddStringArrayItems(srcDirectory, projectBaseName, entities, dbProvider);
        AddValueObjects(srcDirectory, projectBaseName, entities);

        new DbContextModifier(_fileSystem).AddDbSetAndConfig(srcDirectory, entities, dbContextName, projectBaseName);
    }

    private void AddRelationships(string srcDirectory, string projectBaseName, List<Entity> entities)
    {
        // reloop once all bases are added for relationships to mod on top -- could push into the earlier loop if perf becomes an issue
        foreach (var entity in entities)
        {
            var entityModifier = new EntityModifier(_fileSystem, _consoleWriter);
            var allPropsNotNone = entity.Properties.Where(x => !x.GetDbRelationship.IsNone).ToList();
            foreach (var entityProperty in allPropsNotNone)
            {
                entityProperty.GetDbRelationship.UpdateEntityProperties(entityModifier, 
                    srcDirectory,
                    entity.Name,
                    entity.Plural,
                    entityProperty.ForeignEntityName,
                    entityProperty.ForeignEntityPlural,
                    entityProperty.Name,
                    projectBaseName);
                
                entityProperty.GetDbRelationship.UpdateEntityManagementMethods(entityModifier, 
                    srcDirectory,
                    entity.Name,
                    entity.Plural,
                    entityProperty,
                    projectBaseName);
                
                entityModifier.AddParentRelationshipEntity(srcDirectory,
                    entityProperty, 
                    entity.Name, 
                    entity.Plural, 
                    projectBaseName);
                
                new DatabaseEntityConfigModifier(_fileSystem, _consoleWriter).AddRelationships(srcDirectory, 
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
                new DatabaseEntityConfigModifier(_fileSystem, _consoleWriter).AddValueObjectConfig(srcDirectory, 
                    entity.Name,
                    valueObjectProp, 
                    projectBaseName);
            }
        }
        
        var baseValueObjects = new List<EntityProperty>();
        baseValueObjects.Add(new EntityProperty()
        {
            ValueObjectName = "Email",
            AsValueObject = "Email",
        });
        baseValueObjects.Add(new EntityProperty()
        {
            Name = "Percent",
            ValueObjectName = "Percent",
            ValueObjectPlural = "Percentages",
            AsValueObject = "Percent",
        });
        baseValueObjects.Add(new EntityProperty()
        {
            Name = "MonetaryAmount",
            ValueObjectName = "MonetaryAmount",
            AsValueObject = "MonetaryAmount",
        });

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
            
            if (_fileSystem.File.Exists(classPath.FullClassPath))
                continue;
            
            var fileText = valueObjectProp.ValueObjectType.GetFileText(classPath.ClassNamespace, 
                valueObjectProp.ValueObjectName,
                valueObjectProp.Type,
                valueObjectProp.SmartNames,
                srcDirectory,
                projectBaseName);
            _utilities.CreateFile(classPath, fileText);
        }
        
        var entitiesThatHaveValueObjectProperties = entities
            .Where(x => x.Properties.Any(p => p.IsValueObject))
            .ToList();
        foreach (var entityThatHasValueObjectProperties in entitiesThatHaveValueObjectProperties)
        {
            var voProperties = entityThatHasValueObjectProperties.Properties.Where(x => x.IsValueObject).ToList();
            foreach (var entityProperty in voProperties)
            {
                if (entityProperty.Type.ToLower() != "string")
                {
                    new EntityMappingModifier(_fileSystem, _consoleWriter)
                        .UpdateMappingAttributesForValueObject(srcDirectory, 
                            entityThatHasValueObjectProperties.Name,
                            entityThatHasValueObjectProperties.Plural, 
                            entityProperty, 
                            projectBaseName);
                }
            }
        }
    }

    public void AddStringArrayItems(string srcDirectory, string projectBaseName, List<Entity> entities, DbProvider dbProvider)
    {
        foreach (var entity in entities)
        {
            var entityModifier = new EntityModifier(_fileSystem, _consoleWriter);
            var stringArrayProps = entity.Properties.Where(x => x.IsStringArray).ToList();
            foreach (var stringArrayProp in stringArrayProps)
            {
                entityModifier.AddStringArrayManagement(srcDirectory,
                    stringArrayProp,
                    entity.Name,
                    entity.Plural,
                    projectBaseName);
                
                new DatabaseEntityConfigModifier(_fileSystem, _consoleWriter).AddStringArrayProperty(srcDirectory, 
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
                    new() { Type = FeatureType.AddRecord.Name, IsProtected = true },
                    new() { Type = FeatureType.UpdateRecord.Name, IsProtected = true },
                    new() { Type = FeatureType.DeleteRecord.Name, IsProtected = true },
                },
            Properties = new List<EntityProperty>()
                {
                    new() { Name = "Role", Type = "string" },
                    new() { Name = "Permission", Type = "string" }
                }
        };

        new EntityBuilder(_utilities).CreateRolePermissionsEntity(srcDirectory, entity, projectBaseName);
        new DtoBuilder(_utilities, _fileSystem).CreateDtos(srcDirectory, entity, projectBaseName);
        new EntityModelBuilder(_utilities, _fileSystem).CreateEntityModels(srcDirectory, entity, projectBaseName);
        new EntityMappingBuilder(_utilities).CreateMapping(srcDirectory, entity.Name, entity.Plural, projectBaseName);
        new ApiRouteModifier(_fileSystem, _consoleWriter).AddRoutes(testDirectory, entity, projectBaseName);
        _mediator.Send(new DatabaseEntityConfigRolePermissionBuilder.Command());
        
        _mediator.Send(new EntityRepositoryBuilder.Command(dbContextName, 
            entity.Name, 
            entity.Plural));

        if (entity.Features.Count > 0)
            new ControllerBuilder(_utilities).CreateController(solutionDirectory, srcDirectory, entity.Plural, projectBaseName, true);

        // TODO refactor to factory?
        foreach (var feature in entity.Features)
        {
            AddFeatureToProject(solutionDirectory, srcDirectory, testDirectory, projectBaseName, dbContextName, addSwaggerComments, feature, entity, useSoftDelete);
        }

        // Shared Tests
        new FakesBuilder(_utilities).CreateRolePermissionFakes(srcDirectory, solutionDirectory, testDirectory, projectBaseName, entity);
        new RolePermissionsUnitTestBuilder(_utilities).CreateRolePermissionTests(solutionDirectory, testDirectory, srcDirectory, projectBaseName);
        new RolePermissionsUnitTestBuilder(_utilities).UpdateRolePermissionTests(solutionDirectory, testDirectory, srcDirectory, projectBaseName);
        new FakeEntityBuilderBuilder(_utilities).CreateFakeBuilder(srcDirectory, testDirectory, projectBaseName, entity);
        
        // need to do db modifier
        new DbContextModifier(_fileSystem).AddDbSetAndConfig(srcDirectory, new List<Entity>() { entity }, dbContextName, projectBaseName);

        // domain events
        _mediator.Send(new CreatedDomainEventBuilder.CreatedDomainEventBuilderCommand(entity.Name, entity.Plural));
        _mediator.Send(new UpdatedDomainEventBuilder.UpdatedDomainEventBuilderCommand(entity.Name, entity.Plural));
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
            Features = new List<Feature>()
                {
                    new() { Type = FeatureType.GetList.Name, IsProtected = true },
                    new() { Type = FeatureType.GetRecord.Name, IsProtected = true },
                    new() { Type = FeatureType.AddRecord.Name, IsProtected = true },
                    new() { Type = FeatureType.UpdateRecord.Name, IsProtected = true },
                    new() { Type = FeatureType.DeleteRecord.Name, IsProtected = true },
                },
            Properties = new List<EntityProperty>()
                {
                    new() { Name = "Identifier", Type = "string" },
                    new() { Name = "FirstName", Type = "string" },
                    new() { Name = "LastName", Type = "string" },
                    new() { Name = "Email", Type = "string" },
                    new() { Name = "Username", Type = "string" },
                    new() { Name = "UserRoles", Type = "ICollection<UserRole>", ForeignEntityPlural = "UserRoles" },
                }
        };

        var entityBuilder = new EntityBuilder(_utilities);
        entityBuilder.CreateUserEntity(srcDirectory, userEntity, projectBaseName);
        entityBuilder.CreateUserRoleEntity(srcDirectory, projectBaseName);
        
        // TODO custom dto for roles
        new DtoBuilder(_utilities, _fileSystem).CreateDtos(srcDirectory, userEntity, projectBaseName);
        
        new EntityModelBuilder(_utilities, _fileSystem).CreateEntityModels(srcDirectory, userEntity, projectBaseName);
        new EntityMappingBuilder(_utilities).CreateMapping(srcDirectory, "User", "Users", projectBaseName);
        new ApiRouteModifier(_fileSystem, _consoleWriter).AddRoutesForUser(testDirectory, projectBaseName);
        _mediator.Send(new DatabaseEntityConfigUserBuilder.Command());
        _mediator.Send(new DatabaseEntityConfigUserRoleBuilder.Command());
        
        _mediator.Send(new UserEntityRepositoryBuilder.Command(dbContextName, 
            userEntity.Name, 
            userEntity.Plural));
        
        new ControllerBuilder(_utilities).CreateController(solutionDirectory, srcDirectory, userEntity.Plural, projectBaseName, true);
        new ControllerModifier(_fileSystem).AddCustomUserEndpoint(srcDirectory, projectBaseName);
        
        foreach (var feature in userEntity.Features)
        {
            AddFeatureToProject(solutionDirectory, srcDirectory, testDirectory, projectBaseName, dbContextName, addSwaggerComments, feature, userEntity, useSoftDelete);
        }
        new CommandAddUserRoleBuilder(_utilities).CreateCommand(srcDirectory, userEntity, projectBaseName);
        new CommandRemoveUserRoleBuilder(_utilities).CreateCommand(srcDirectory, userEntity, projectBaseName);
        // new AddUserFeatureBuilder(_utilities).AddFeature(srcDirectory, projectBaseName);
        new AddUserFeatureOverrideModifier(_fileSystem).UpdateAddUserFeature(srcDirectory, projectBaseName);

        // extra testing
        new FakesBuilder(_utilities).CreateUserFakes(srcDirectory, solutionDirectory, testDirectory, projectBaseName, userEntity);
        new CreateUserRoleUnitTestBuilder(_utilities).CreateTests(solutionDirectory, testDirectory, srcDirectory, projectBaseName);
        new AddRemoveUserRoleTestsBuilder(_utilities).CreateTests(testDirectory, srcDirectory, projectBaseName);
        new UserUnitTestBuilder(_utilities).CreateTests(solutionDirectory, testDirectory, srcDirectory, projectBaseName);
        new UserUnitTestBuilder(_utilities).UpdateTests(solutionDirectory, testDirectory, srcDirectory, projectBaseName);
        new FakeEntityBuilderBuilder(_utilities).CreateFakeBuilder(srcDirectory, testDirectory, projectBaseName, userEntity);
        
        // need to do db modifier
        new DbContextModifier(_fileSystem).AddDbSetAndConfig(srcDirectory, new List<Entity>() { userEntity }, dbContextName, projectBaseName);
        new DbContextModifier(_fileSystem).AddDbSetAndConfig(srcDirectory, new List<Entity>() { new Entity()
        {
            Name = "UserRole",
            Plural = "UserRoles"
        } }, dbContextName, projectBaseName);

        // domain events
        _mediator.Send(new CreatedDomainEventBuilder.CreatedDomainEventBuilderCommand(userEntity.Name, userEntity.Plural));
        _mediator.Send(new UpdatedDomainEventBuilder.UpdatedDomainEventBuilderCommand(userEntity.Name, userEntity.Plural));
        _mediator.Send(new UpdatedUserRoleDomainEventBuilder.Command());
    }

    public void AddFeatureToProject(string solutionDirectory, string srcDirectory, string testDirectory, string projectBaseName,
        string dbContextName, bool addSwaggerComments, Feature feature, Entity entity, bool useSoftDelete)
    {
        var controllerClassPath = ClassPathHelper.ControllerClassPath(srcDirectory, $"{FileNames.GetControllerName(entity.Plural)}.cs", projectBaseName);
        if (!File.Exists(controllerClassPath.FullClassPath))
            new ControllerBuilder(_utilities).CreateController(solutionDirectory, srcDirectory, entity.Plural, projectBaseName, feature.IsProtected);

        if (feature.IsProtected)
            new PermissionsModifier(_fileSystem).AddPermission(srcDirectory, feature.PermissionName, projectBaseName);

        if (feature.Type == FeatureType.AddRecord.Name)
        {
            new CommandAddRecordBuilder(_utilities).CreateCommand(srcDirectory, entity, projectBaseName, feature.IsProtected, feature.PermissionName);
            if(entity.Name == "RolePermission")
                new Craftsman.Builders.Tests.IntegrationTests.RolePermissions.AddCommandTestBuilder(_utilities).CreateTests(testDirectory, srcDirectory, entity, projectBaseName);
            else if(entity.Name == "User")
                new Craftsman.Builders.Tests.IntegrationTests.Users.AddCommandTestBuilder(_utilities).CreateTests(testDirectory, srcDirectory, entity, projectBaseName);
            else
                new AddCommandTestBuilder(_utilities).CreateTests(testDirectory, srcDirectory, entity, projectBaseName, feature.PermissionName, feature.IsProtected);

            new CreateEntityTestBuilder(_utilities).CreateTests(solutionDirectory, testDirectory, entity, feature.IsProtected, projectBaseName);
            new ControllerModifier(_fileSystem).AddEndpoint(srcDirectory, FeatureType.AddRecord, entity, addSwaggerComments,
                feature, projectBaseName);
        }

        if (feature.Type == FeatureType.GetRecord.Name)
        {
            new QueryGetRecordBuilder(_utilities).CreateQuery(srcDirectory, entity, projectBaseName, feature.IsProtected, feature.PermissionName);
            if(entity.Name == "RolePermission")
                new Craftsman.Builders.Tests.IntegrationTests.RolePermissions.GetRecordQueryTestBuilder(_utilities).CreateTests(solutionDirectory, testDirectory, srcDirectory, entity, projectBaseName);
            else if(entity.Name == "User")
                new Craftsman.Builders.Tests.IntegrationTests.Users.GetRecordQueryTestBuilder(_utilities).CreateTests(solutionDirectory, testDirectory, srcDirectory, entity, projectBaseName);
            else
                new GetRecordQueryTestBuilder(_utilities).CreateTests(solutionDirectory, testDirectory, srcDirectory, entity, projectBaseName, feature.PermissionName, feature.IsProtected);

            new GetEntityRecordTestBuilder(_utilities).CreateTests(solutionDirectory, testDirectory, entity, feature.IsProtected, projectBaseName);
            new ControllerModifier(_fileSystem).AddEndpoint(srcDirectory, FeatureType.GetRecord, entity, addSwaggerComments,
                feature, projectBaseName);
        }

        if (feature.Type == FeatureType.GetList.Name)
        {
            new QueryGetListBuilder(_utilities).CreateQuery(srcDirectory, entity, projectBaseName, feature.IsProtected, feature.PermissionName);
            new GetListQueryTestBuilder(_utilities).CreateTests(testDirectory, srcDirectory, entity, projectBaseName, feature.PermissionName, feature.IsProtected);
            new GetEntityListTestBuilder(_utilities).CreateTests(solutionDirectory, testDirectory, entity, feature.IsProtected, projectBaseName);
            new ControllerModifier(_fileSystem).AddEndpoint(srcDirectory, FeatureType.GetList, entity, addSwaggerComments,
                feature, projectBaseName);
        }

        if (feature.Type == FeatureType.GetAll.Name)
        {
            new QueryGetAllBuilder(_utilities).CreateQuery(srcDirectory, entity, projectBaseName, feature.IsProtected, feature.PermissionName);
            new GetAllQueryTestBuilder(_utilities).CreateTests(testDirectory, srcDirectory, entity, projectBaseName, feature.PermissionName, feature.IsProtected);
            new GetAllEntitiesTestBuilder(_utilities).CreateTests(solutionDirectory, testDirectory, entity, feature.IsProtected, projectBaseName);
            new ControllerModifier(_fileSystem).AddEndpoint(srcDirectory, FeatureType.GetAll, entity, addSwaggerComments,
                feature, projectBaseName);
        }

        if (feature.Type == FeatureType.DeleteRecord.Name)
        {
            new CommandDeleteRecordBuilder(_utilities).CreateCommand(srcDirectory, entity, projectBaseName, feature.IsProtected, feature.PermissionName);
            new DeleteCommandTestBuilder(_utilities).CreateTests(solutionDirectory, testDirectory, srcDirectory, entity, projectBaseName, useSoftDelete, feature.PermissionName, feature.IsProtected);
            new DeleteEntityTestBuilder(_utilities).CreateTests(solutionDirectory, testDirectory, entity, feature.IsProtected, projectBaseName);
            new ControllerModifier(_fileSystem).AddEndpoint(srcDirectory, FeatureType.DeleteRecord, entity, addSwaggerComments,
                feature, projectBaseName);
        }

        if (feature.Type == FeatureType.UpdateRecord.Name)
        {
            new CommandUpdateRecordBuilder(_utilities).CreateCommand(srcDirectory, entity, projectBaseName, feature.IsProtected, feature.PermissionName);
            
            if(entity.Name == "RolePermission")
                new Craftsman.Builders.Tests.IntegrationTests.RolePermissions.PutCommandTestBuilder(_utilities).CreateTests(solutionDirectory, testDirectory, srcDirectory, entity, projectBaseName);
            else if(entity.Name == "User")
                new Craftsman.Builders.Tests.IntegrationTests.Users.PutCommandTestBuilder(_utilities).CreateTests(solutionDirectory, testDirectory, srcDirectory, entity, projectBaseName);
            else
                new PutCommandTestBuilder(_utilities).CreateTests(solutionDirectory, testDirectory, srcDirectory, entity, projectBaseName, feature.IsProtected, feature.PermissionName);
            
            new PutEntityTestBuilder(_utilities).CreateTests(solutionDirectory, testDirectory, entity, feature.IsProtected, projectBaseName);
            new ControllerModifier(_fileSystem).AddEndpoint(srcDirectory, FeatureType.UpdateRecord, entity, addSwaggerComments,
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
            new CommandAddListBuilder(_utilities).CreateCommand(srcDirectory, entity, projectBaseName, feature, feature.IsProtected, feature.PermissionName);
            new AddListCommandTestBuilder(_utilities).CreateTests(solutionDirectory, testDirectory, srcDirectory, entity, feature, projectBaseName, feature.PermissionName, feature.IsProtected);
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

        if (feature.Type == FeatureType.Job.Name)
        {
            _mediator.Send(new JobFeatureBuilder.Command(feature, entity.Plural));
            _mediator.Send(new JobFeatureIntegrationTestBuilder.Command(feature, entity.Plural));
        }
    }
}
