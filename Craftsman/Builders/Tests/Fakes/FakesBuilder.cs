namespace Craftsman.Builders.Tests.Fakes;

using System;
using System.IO;
using Domain;
using Domain.Enums;
using Helpers;
using Services;
using MediatR;

public static class FakesBuilder
{
    public sealed record CreateFakesCommand(Entity Entity) : IRequest;
    public sealed record CreateRolePermissionFakesCommand(Entity Entity) : IRequest;
    public sealed record CreateUserFakesCommand(Entity Entity) : IRequest;
    public sealed record CreateAddressFakesCommand() : IRequest;

    public class CreateFakesHandler(ICraftsmanUtilities utilities, IScaffoldingDirectoryStore scaffoldingDirectoryStore) : IRequestHandler<CreateFakesCommand>
    {
        public Task Handle(CreateFakesCommand request, CancellationToken cancellationToken)
        {
            CreateFakes(scaffoldingDirectoryStore.SrcDirectory, scaffoldingDirectoryStore.TestDirectory, scaffoldingDirectoryStore.ProjectBaseName, request.Entity, utilities);
            return Task.CompletedTask;
        }
    }

    public class CreateRolePermissionFakesHandler(ICraftsmanUtilities utilities, IScaffoldingDirectoryStore scaffoldingDirectoryStore) : IRequestHandler<CreateRolePermissionFakesCommand>
    {
        public Task Handle(CreateRolePermissionFakesCommand request, CancellationToken cancellationToken)
        {
            CreateRolePermissionFakes(scaffoldingDirectoryStore.SrcDirectory, scaffoldingDirectoryStore.SolutionDirectory, scaffoldingDirectoryStore.TestDirectory, scaffoldingDirectoryStore.ProjectBaseName, request.Entity, utilities);
            return Task.CompletedTask;
        }
    }

    public class CreateUserFakesHandler(ICraftsmanUtilities utilities, IScaffoldingDirectoryStore scaffoldingDirectoryStore) : IRequestHandler<CreateUserFakesCommand>
    {
        public Task Handle(CreateUserFakesCommand request, CancellationToken cancellationToken)
        {
            CreateUserFakes(scaffoldingDirectoryStore.SrcDirectory, scaffoldingDirectoryStore.SolutionDirectory, scaffoldingDirectoryStore.TestDirectory, scaffoldingDirectoryStore.ProjectBaseName, request.Entity, utilities);
            return Task.CompletedTask;
        }
    }

    public class CreateAddressFakesHandler(ICraftsmanUtilities utilities, IScaffoldingDirectoryStore scaffoldingDirectoryStore) : IRequestHandler<CreateAddressFakesCommand>
    {
        public Task Handle(CreateAddressFakesCommand request, CancellationToken cancellationToken)
        {
            CreateAddressFakes(scaffoldingDirectoryStore.SrcDirectory, scaffoldingDirectoryStore.TestDirectory, scaffoldingDirectoryStore.ProjectBaseName, utilities);
            return Task.CompletedTask;
        }
    }

    private static void CreateFakes(string srcDirectory, string testDirectory, string projectBaseName, Entity entity, ICraftsmanUtilities utilities)
    {
        // ****this class path will have an invalid FullClassPath. just need the directory
        var classPath = ClassPathHelper.TestFakesClassPath(testDirectory, $"", entity.Name, projectBaseName);

        if (!Directory.Exists(classPath.ClassDirectory))
            Directory.CreateDirectory(classPath.ClassDirectory);

        CreateFakerFile(srcDirectory, testDirectory, Dto.Creation, entity, projectBaseName, utilities);
        CreateFakerFile(srcDirectory, testDirectory, Dto.Update, entity, projectBaseName, utilities);
        CreateFakerFile(srcDirectory, testDirectory, EntityModel.Creation, entity, projectBaseName, utilities);
        CreateFakerFile(srcDirectory, testDirectory, EntityModel.Update, entity, projectBaseName, utilities);
    }

    private static void CreateRolePermissionFakes(string srcDirectory, string solutionDirectory, string testDirectory, string projectBaseName, Entity entity, ICraftsmanUtilities utilities)
    {
        var classPath = ClassPathHelper.TestFakesClassPath(testDirectory, $"", entity.Name, projectBaseName);

        if (!Directory.Exists(classPath.ClassDirectory))
            Directory.CreateDirectory(classPath.ClassDirectory);

        CreateRolePermissionFakerForCreationOrUpdateFile(srcDirectory, solutionDirectory, testDirectory, FileNames.GetDtoName(entity.Name, Dto.Creation), entity, projectBaseName, utilities);
        CreateRolePermissionFakerForCreationOrUpdateFile(srcDirectory, solutionDirectory, testDirectory, FileNames.GetDtoName(entity.Name, Dto.Update), entity, projectBaseName, utilities);
        CreateRolePermissionFakerForCreationOrUpdateFile(srcDirectory, solutionDirectory, testDirectory, EntityModel.Creation.GetClassName(entity.Name), entity, projectBaseName, utilities);
        CreateRolePermissionFakerForCreationOrUpdateFile(srcDirectory, solutionDirectory, testDirectory, EntityModel.Update.GetClassName(entity.Name), entity, projectBaseName, utilities);
    }
    
    private static void CreateUserFakes(string srcDirectory, string solutionDirectory, string testDirectory, string projectBaseName, Entity entity, ICraftsmanUtilities utilities)
    {
        var classPath = ClassPathHelper.TestFakesClassPath(testDirectory, $"", entity.Name, projectBaseName);

        if (!Directory.Exists(classPath.ClassDirectory))
            Directory.CreateDirectory(classPath.ClassDirectory);

        CreateUserFakerForCreationOrUpdateFile(srcDirectory, solutionDirectory, testDirectory, FileNames.GetDtoName(entity.Name, Dto.Creation), entity, projectBaseName, utilities);
        CreateUserFakerForCreationOrUpdateFile(srcDirectory, solutionDirectory, testDirectory, FileNames.GetDtoName(entity.Name, Dto.Update), entity, projectBaseName, utilities);
        CreateUserFakerForCreationOrUpdateFile(srcDirectory, solutionDirectory, testDirectory, EntityModel.Creation.GetClassName(entity.Name), entity, projectBaseName, utilities);
        CreateUserFakerForCreationOrUpdateFile(srcDirectory, solutionDirectory, testDirectory, EntityModel.Update.GetClassName(entity.Name), entity, projectBaseName, utilities);
    }

    private static void CreateAddressFakes(string srcDirectory, string testDirectory, string projectBaseName, ICraftsmanUtilities utilities)
    {
        var entity = new Entity();
        entity.Name = "Address";
        entity.Plural = "Addresses";
        var classPath = ClassPathHelper.TestFakesClassPath(testDirectory, $"", entity.Name, projectBaseName);

        if (!Directory.Exists(classPath.ClassDirectory))
            Directory.CreateDirectory(classPath.ClassDirectory);

        CreateAddressFakerForReadDtoFile(srcDirectory, testDirectory, projectBaseName, utilities);

        CreateAddressFakerForCreationOrUpdateFile(srcDirectory, testDirectory, FileNames.GetDtoName(entity.Name, Dto.Creation), entity, projectBaseName, utilities);
        CreateAddressFakerForCreationOrUpdateFile(srcDirectory, testDirectory, FileNames.GetDtoName(entity.Name, Dto.Update), entity, projectBaseName, utilities);
    }

    private static void CreateFakerFile(string srcDirectory, string testDirectory, Dto dtoType, Entity entity, string projectBaseName, ICraftsmanUtilities utilities)
    {
        var objectToFakeClassName = FileNames.GetDtoName(entity.Name, dtoType);
        var fakeFilename = $"Fake{objectToFakeClassName}.cs";
        var classPath = ClassPathHelper.TestFakesClassPath(testDirectory, fakeFilename, entity.Name, projectBaseName);
        var fileText = GetFakeFileText(classPath.ClassNamespace, dtoType, entity, srcDirectory, testDirectory, projectBaseName);
        utilities.CreateFile(classPath, fileText);
    }

    private static string GetFakeFileText(string classNamespace, Dto dtoType, Entity entity, string srcDirectory, string testDirectory, string projectBaseName)
    {
        var objectToFakeClassName = FileNames.GetDtoName(entity.Name, dtoType);
        var entitiesClassPath = ClassPathHelper.EntityClassPath(testDirectory, "", entity.Plural, projectBaseName);
        var dtoClassPath = ClassPathHelper.DtoClassPath(srcDirectory, "", entity.Plural, projectBaseName);

        var rulesFor = "";

        // this... is super fragile. Should really refactor this
        var usingStatement = string.Empty;
        if (objectToFakeClassName.Contains("DTO", StringComparison.InvariantCultureIgnoreCase))
            usingStatement = @$"using {dtoClassPath.ClassNamespace};";
        foreach (var entityProperty in entity.Properties)
        {
            if (entityProperty.ValueObjectType.IsEmail)
            {
                rulesFor += @$"
        RuleFor({entity.Lambda} => {entity.Lambda}.{entityProperty.Name}, f => f.Person.Email);";
            }

            if (entityProperty.ValueObjectType.IsSmart)
            {
                rulesFor += @$"
        RuleFor({entity.Lambda} => {entity.Lambda}.{entityProperty.Name}, f => f.PickRandom({entityProperty.ValueObjectName}.ListNames()));";
            
            var voClassPath = ClassPathHelper.ValueObjectClassPath(srcDirectory, entityProperty.ValueObjectType.ClassName, entityProperty.ValueObjectPlural, projectBaseName);
            usingStatement += $@"
using {voClassPath.ClassNamespace};";
            }
        }
        
        return @$"namespace {classNamespace};

using AutoBogus;
using {entitiesClassPath.ClassNamespace};
{usingStatement}

public sealed class Fake{objectToFakeClassName} : AutoFaker<{objectToFakeClassName}>
{{
    public Fake{objectToFakeClassName}()
    {{{rulesFor}
    }}
}}";
    }

    private static void CreateFakerFile(string srcDirectory, string testDirectory, EntityModel modelType, Entity entity, string projectBaseName, ICraftsmanUtilities utilities)
    {
        var objectToFakeClassName = modelType.GetClassName(entity.Name);
        var fakeFilename = $"Fake{objectToFakeClassName}.cs";
        var classPath = ClassPathHelper.TestFakesClassPath(testDirectory, fakeFilename, entity.Name, projectBaseName);
        var fileText = GetFakeFileText(classPath.ClassNamespace, modelType, entity, srcDirectory, testDirectory, projectBaseName);
        utilities.CreateFile(classPath, fileText);
    }
    
    private static string GetFakeFileText(string classNamespace, EntityModel modelType, Entity entity, string srcDirectory, string testDirectory, string projectBaseName)
    {
        var objectToFakeClassName = modelType.GetClassName(entity.Name);
        var entitiesClassPath = ClassPathHelper.EntityClassPath(testDirectory, "", entity.Plural, projectBaseName);
        var modelClassPath = ClassPathHelper.EntityModelClassPath(srcDirectory, entity.Name, entity.Plural, null, projectBaseName);

        var rulesFor = "";

        var usingStatement = @$"using {modelClassPath.ClassNamespace};";
        foreach (var entityProperty in entity.Properties)
        {
            if (entityProperty.ValueObjectType.IsEmail)
            {
                rulesFor += @$"
        RuleFor({entity.Lambda} => {entity.Lambda}.{entityProperty.Name}, f => f.Person.Email);";
            }

            if (entityProperty.ValueObjectType.IsSmart)
            {
                rulesFor += @$"
        RuleFor({entity.Lambda} => {entity.Lambda}.{entityProperty.Name}, f => f.PickRandom({entityProperty.ValueObjectName}.ListNames()));";
            
            var voClassPath = ClassPathHelper.ValueObjectClassPath(srcDirectory, entityProperty.ValueObjectType.ClassName, entityProperty.ValueObjectPlural, projectBaseName);
            usingStatement += $@"
using {voClassPath.ClassNamespace};";
            }
        }

        return @$"namespace {classNamespace};

using AutoBogus;
using {entitiesClassPath.ClassNamespace};
{usingStatement}

public sealed class Fake{objectToFakeClassName} : AutoFaker<{objectToFakeClassName}>
{{
    public Fake{objectToFakeClassName}()
    {{{rulesFor}
    }}
}}";
    }

    private static void CreateRolePermissionFakerForCreationOrUpdateFile(string srcDirectory, string solutionDirectory, string testDirectory, string objectToFakeClassName, Entity entity, string projectBaseName, ICraftsmanUtilities utilities)
    {
        var fakeFilename = $"Fake{objectToFakeClassName}.cs";
        var classPath = ClassPathHelper.TestFakesClassPath(testDirectory, fakeFilename, entity.Name, projectBaseName);

        var dtoClassPath = ClassPathHelper.DtoClassPath(srcDirectory, "", entity.Plural, projectBaseName);
        var policyDomainClassPath = ClassPathHelper.PolicyDomainClassPath(testDirectory, "", projectBaseName);
        var rolesClassPath = ClassPathHelper.EntityClassPath(solutionDirectory, "", "Roles", projectBaseName);
        var modelClassPath = ClassPathHelper.EntityModelClassPath(srcDirectory, entity.Name, entity.Plural, null, projectBaseName);

        var fileText = @$"namespace {classPath.ClassNamespace};

using AutoBogus;
using {policyDomainClassPath.ClassNamespace};
using {dtoClassPath.ClassNamespace};
using {rolesClassPath.ClassNamespace};
using {modelClassPath.ClassNamespace};

public sealed class Fake{objectToFakeClassName} : AutoFaker<{objectToFakeClassName}>
{{
    public Fake{objectToFakeClassName}()
    {{
        RuleFor(rp => rp.Permission, f => f.PickRandom(Permissions.List()));
        RuleFor(rp => rp.Role, f => f.PickRandom(Role.ListNames()));
    }}
}}";

        utilities.CreateFile(classPath, fileText);
    }


    private static void CreateUserFakerForCreationOrUpdateFile(string srcDirectory, string solutionDirectory, string testDirectory, string objectToFakeClassName, Entity entity, string projectBaseName, ICraftsmanUtilities utilities)
    {
        var fakeFilename = $"Fake{objectToFakeClassName}.cs";
        var classPath = ClassPathHelper.TestFakesClassPath(testDirectory, fakeFilename, entity.Name, projectBaseName);

        var dtoClassPath = ClassPathHelper.DtoClassPath(srcDirectory, "", entity.Plural, projectBaseName);
        var policyDomainClassPath = ClassPathHelper.PolicyDomainClassPath(testDirectory, "", projectBaseName);
        var rolesClassPath = ClassPathHelper.EntityClassPath(solutionDirectory, "", "Roles", projectBaseName);
        var modelClassPath = ClassPathHelper.EntityModelClassPath(srcDirectory, entity.Name, entity.Plural, null, projectBaseName);

        var fileText = @$"namespace {classPath.ClassNamespace};

using AutoBogus;
using {policyDomainClassPath.ClassNamespace};
using {dtoClassPath.ClassNamespace};
using {rolesClassPath.ClassNamespace};
using {modelClassPath.ClassNamespace};

public sealed class Fake{objectToFakeClassName} : AutoFaker<{objectToFakeClassName}>
{{
    public Fake{objectToFakeClassName}()
    {{
        RuleFor(u => u.Email, f => f.Person.Email);
    }}
}}";

        utilities.CreateFile(classPath, fileText);
    }
    private static void CreateAddressFakerForCreationOrUpdateFile(string srcDirectory, string testDirectory, string objectToFakeClassName, Entity entity, string projectBaseName, ICraftsmanUtilities utilities)
    {
        var fakeFilename = $"Fake{objectToFakeClassName}.cs";
        var classPath = ClassPathHelper.TestFakesClassPath(testDirectory, fakeFilename, entity.Name, projectBaseName);

        var dtoClassPath = ClassPathHelper.DtoClassPath(srcDirectory, "", entity.Plural, projectBaseName);

        var fileText = @$"namespace {classPath.ClassNamespace};

using AutoBogus;
using {dtoClassPath.ClassNamespace};

public sealed class Fake{objectToFakeClassName} : AutoFaker<{objectToFakeClassName}>
{{
    public Fake{objectToFakeClassName}()
    {{
        RuleFor(u => u.Line1, f => f.Address.StreetAddress());
        RuleFor(u => u.Line2, f => f.Address.SecondaryAddress());
        RuleFor(u => u.City, f => f.Address.City());
        RuleFor(u => u.State, f => f.Address.State());
        RuleFor(u => u.PostalCode, f => f.Address.ZipCode());
        RuleFor(u => u.Country, f => f.Address.Country());
    }}
}}";

        utilities.CreateFile(classPath, fileText);
    }
    
    private static void CreateAddressFakerForReadDtoFile(string srcDirectory, string testDirectory, string projectBaseName, ICraftsmanUtilities utilities)
    {
        var fakeFilename = "FakeAddress.cs";
        var classPath = ClassPathHelper.TestFakesClassPath(testDirectory, fakeFilename, "Address", projectBaseName);

        var dtoClassPath = ClassPathHelper.DtoClassPath(srcDirectory, "", "Addresses", projectBaseName);
        var entityClassPath = ClassPathHelper.EntityClassPath(srcDirectory, "", "Addresses", projectBaseName);

        var fileText = @$"namespace {classPath.ClassNamespace};

using {dtoClassPath.ClassNamespace};
using {entityClassPath.ClassNamespace};

public class FakeAddress
{{
    public static Address Generate(AddressForCreationDto addressForCreationDto)
    {{
        return new Address(addressForCreationDto.Line1,
            addressForCreationDto.Line2,
            addressForCreationDto.City,
            addressForCreationDto.State,
            addressForCreationDto.PostalCode,
            addressForCreationDto.Country);
    }}

    public static Address Generate()
    {{
        return Generate(new FakeAddressForCreationDto().Generate());
    }}
}}";

        utilities.CreateFile(classPath, fileText);
    }
}
