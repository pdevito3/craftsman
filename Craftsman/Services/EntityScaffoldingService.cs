namespace Craftsman.Services;

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
using Builders.Tests.IntegrationTests.UserRoles;
using Builders.Tests.UnitTests;
using Builders.Tests.Utilities;
using Domain;
using Domain.Enums;
using Helpers;
using MediatR;

public class EntityScaffoldingService
{
    private readonly IFileSystem _fileSystem;
    private readonly ICraftsmanUtilities _utilities;
    private readonly IMediator _mediator;

    public EntityScaffoldingService(ICraftsmanUtilities utilities, IFileSystem fileSystem, IMediator mediator)
    {
        _fileSystem = fileSystem;
        _mediator = mediator;
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
            new DtoBuilder(_utilities, _fileSystem).CreateDtos(srcDirectory, entity, projectBaseName);
            new ValidatorBuilder(_utilities).CreateValidators(solutionDirectory, srcDirectory, projectBaseName, entity);
            new EntityMappingBuilder(_utilities).CreateMapping(srcDirectory, entity, projectBaseName);
            new ApiRouteModifier(_fileSystem).AddRoutes(testDirectory, entity, projectBaseName); // api routes always added to testing by default. too much of a pain to scaffold dynamically

            _mediator.Send(new DatabaseEntityConfigBuilder.Command(entity.Name, entity.Plural));
            _mediator.Send(new EntityRepositoryBuilder.Command(dbContextName, 
                entity.Name, 
                entity.Plural));

            var smartProps = entity.Properties.Where(x => x.IsSmartEnum()).ToList();
            foreach (var smartProp in smartProps)
                _mediator.Send(new SmartEnumBuilder.SmartEnumBuilderCommand(smartProp, entity.Plural));

            var isProtected = entity.Features.Any(f => f.IsProtected); // <-- one more example of why it would be nice to have specific endpoints for each feature 😤
            if (entity.Features.Count > 0)
                new ControllerBuilder(_utilities).CreateController(solutionDirectory, srcDirectory, entity.Name, entity.Plural, projectBaseName, isProtected);

            // TODO refactor to factory?
            foreach (var feature in entity.Features)
                AddFeatureToProject(solutionDirectory, srcDirectory, testDirectory, projectBaseName, dbContextName, addSwaggerComments, feature, entity, useSoftDelete);

            // Shared Tests
            new FakesBuilder(_utilities).CreateFakes(srcDirectory, testDirectory, projectBaseName, entity);
            new CreateEntityUnitTestBuilder(_utilities)
                .CreateTests(solutionDirectory, testDirectory, srcDirectory, entity.Name, entity.Plural, entity.Properties, projectBaseName);
            new UpdateEntityUnitTestBuilder(_utilities)
                .CreateTests(solutionDirectory, testDirectory, srcDirectory, entity.Name, entity.Plural, entity.Properties, projectBaseName);

            // domain events
            _mediator.Send(new CreatedDomainEventBuilder.CreatedDomainEventBuilderCommand(entity.Name, entity.Plural));
            _mediator.Send(new UpdatedDomainEventBuilder.UpdatedDomainEventBuilderCommand(entity.Name, entity.Plural));
        }

        new DbContextModifier(_fileSystem).AddDbSetAndConfig(srcDirectory, entities, dbContextName, projectBaseName);
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
                    new() { Name = "Role", Type = "string", CanFilter = true, CanSort = true },
                    new() { Name = "Permission", Type = "string", CanFilter = true, CanSort = true }
                }
        };

        new EntityBuilder(_utilities).CreateRolePermissionsEntity(srcDirectory, entity, projectBaseName);
        new DtoBuilder(_utilities, _fileSystem).CreateDtos(srcDirectory, entity, projectBaseName);
        new EntityMappingBuilder(_utilities).CreateMapping(srcDirectory, entity, projectBaseName);
        new ApiRouteModifier(_fileSystem).AddRoutes(testDirectory, entity, projectBaseName);
        _mediator.Send(new DatabaseEntityConfigRolePermissionBuilder.Command());
        
        _mediator.Send(new EntityRepositoryBuilder.Command(dbContextName, 
            entity.Name, 
            entity.Plural));

        // custom validator
        new ValidatorBuilder(_utilities).CreateRolePermissionValidators(solutionDirectory, srcDirectory, projectBaseName, entity);

        if (entity.Features.Count > 0)
            new ControllerBuilder(_utilities).CreateController(solutionDirectory, srcDirectory, entity.Name, entity.Plural, projectBaseName, true);

        // TODO refactor to factory?
        foreach (var feature in entity.Features)
        {
            AddFeatureToProject(solutionDirectory, srcDirectory, testDirectory, projectBaseName, dbContextName, addSwaggerComments, feature, entity, useSoftDelete);
        }

        // Shared Tests
        new FakesBuilder(_utilities).CreateRolePermissionFakes(srcDirectory, solutionDirectory, testDirectory, projectBaseName, entity);
        new RolePermissionsUnitTestBuilder(_utilities).CreateRolePermissionTests(solutionDirectory, testDirectory, srcDirectory, projectBaseName);
        new RolePermissionsUnitTestBuilder(_utilities).UpdateRolePermissionTests(solutionDirectory, testDirectory, srcDirectory, projectBaseName);

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
                    new() { Name = "Identifier", Type = "string", CanFilter = false, CanSort = false },
                    new() { Name = "FirstName", Type = "string", CanFilter = false, CanSort = false },
                    new() { Name = "LastName", Type = "string", CanFilter = false, CanSort = false },
                    new() { Name = "Email", Type = "string", CanFilter = false, CanSort = false },
                    new() { Name = "Username", Type = "string", CanFilter = false, CanSort = false },
                    new() { Name = "UserRoles", Type = "ICollection<UserRole>", ForeignEntityPlural = "UserRoles", CanFilter = false, CanSort = false },
                }
        };

        var entityBuilder = new EntityBuilder(_utilities);
        entityBuilder.CreateUserEntity(srcDirectory, userEntity, projectBaseName);
        entityBuilder.CreateUserRoleEntity(srcDirectory, projectBaseName);
        
        // TODO custom dto for roles
        new DtoBuilder(_utilities, _fileSystem).CreateDtos(srcDirectory, userEntity, projectBaseName);
        
        new EntityMappingBuilder(_utilities).CreateUserMapping(srcDirectory, projectBaseName);
        new ApiRouteModifier(_fileSystem).AddRoutesForUser(testDirectory, projectBaseName);
        _mediator.Send(new DatabaseEntityConfigUserBuilder.Command());
        _mediator.Send(new DatabaseEntityConfigUserRoleBuilder.Command());
        
        _mediator.Send(new UserEntityRepositoryBuilder.Command(dbContextName, 
            userEntity.Name, 
            userEntity.Plural));

        new ValidatorBuilder(_utilities).CreateUserValidators(solutionDirectory, srcDirectory, projectBaseName, userEntity);
        
        new ControllerBuilder(_utilities).CreateController(solutionDirectory, srcDirectory, userEntity.Name, userEntity.Plural, projectBaseName, true);
        new ControllerModifier(_fileSystem).AddCustomUserEndpoint(srcDirectory, projectBaseName);
        
        foreach (var feature in userEntity.Features)
        {
            AddFeatureToProject(solutionDirectory, srcDirectory, testDirectory, projectBaseName, dbContextName, addSwaggerComments, feature, userEntity, useSoftDelete);
        }
        new CommandAddUserRoleBuilder(_utilities).CreateCommand(srcDirectory, userEntity, projectBaseName);
        new CommandRemoveUserRoleBuilder(_utilities).CreateCommand(srcDirectory, userEntity, projectBaseName);
        new AddUserFeatureOverrideModifier(_fileSystem).UpdateAddUserFeature(srcDirectory, projectBaseName);

        // extra testing
        new FakesBuilder(_utilities).CreateUserFakes(srcDirectory, solutionDirectory, testDirectory, projectBaseName, userEntity);
        new CreateUserRoleUnitTestBuilder(_utilities).CreateTests(solutionDirectory, testDirectory, srcDirectory, projectBaseName);
        new AddRemoveUserRoleTestsBuilder(_utilities).CreateTests(testDirectory, srcDirectory, projectBaseName);
        new UserUnitTestBuilder(_utilities).CreateTests(solutionDirectory, testDirectory, srcDirectory, projectBaseName);
        new UserUnitTestBuilder(_utilities).UpdateTests(solutionDirectory, testDirectory, srcDirectory, projectBaseName);
        
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
            new ControllerBuilder(_utilities).CreateController(solutionDirectory, srcDirectory, entity.Name, entity.Plural, projectBaseName, feature.IsProtected);

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
                new AddCommandTestBuilder(_utilities).CreateTests(testDirectory, srcDirectory, entity, projectBaseName);

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
                new GetRecordQueryTestBuilder(_utilities).CreateTests(solutionDirectory, testDirectory, srcDirectory, entity, projectBaseName);

            new GetEntityRecordTestBuilder(_utilities).CreateTests(solutionDirectory, testDirectory, entity, feature.IsProtected, projectBaseName);
            new ControllerModifier(_fileSystem).AddEndpoint(srcDirectory, FeatureType.GetRecord, entity, addSwaggerComments,
                feature, projectBaseName);
        }

        if (feature.Type == FeatureType.GetList.Name)
        {
            new QueryGetListBuilder(_utilities).CreateQuery(srcDirectory, entity, projectBaseName, feature.IsProtected, feature.PermissionName);
            new GetListQueryTestBuilder(_utilities).CreateTests(testDirectory, srcDirectory, entity, projectBaseName);
            new GetEntityListTestBuilder(_utilities).CreateTests(solutionDirectory, testDirectory, entity, feature.IsProtected, projectBaseName);
            new ControllerModifier(_fileSystem).AddEndpoint(srcDirectory, FeatureType.GetList, entity, addSwaggerComments,
                feature, projectBaseName);
            new GetEntityListUnitTestBuilder(_utilities)
                .CreateTests(solutionDirectory, testDirectory, srcDirectory, entity.Name, entity.Plural, entity.Lambda, entity.Properties, projectBaseName, feature.IsProtected);
        }

        if (feature.Type == FeatureType.DeleteRecord.Name)
        {
            new CommandDeleteRecordBuilder(_utilities).CreateCommand(srcDirectory, entity, projectBaseName, feature.IsProtected, feature.PermissionName);
            new DeleteCommandTestBuilder(_utilities).CreateTests(solutionDirectory, testDirectory, srcDirectory, entity, projectBaseName, useSoftDelete);
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
                new PutCommandTestBuilder(_utilities).CreateTests(solutionDirectory, testDirectory, srcDirectory, entity, projectBaseName);
            
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
