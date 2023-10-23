namespace Craftsman.Builders.Tests.Fakes;

using System;
using System.IO;
using Domain;
using Domain.Enums;
using Helpers;
using Services;

public sealed class FakesBuilder
{
    private readonly ICraftsmanUtilities _utilities;

    public FakesBuilder(ICraftsmanUtilities utilities)
    {
        _utilities = utilities;
    }

    public void CreateFakes(string srcDirectory, string testDirectory, string projectBaseName, Entity entity)
    {
        // ****this class path will have an invalid FullClassPath. just need the directory
        var classPath = ClassPathHelper.TestFakesClassPath(testDirectory, $"", entity.Name, projectBaseName);

        if (!Directory.Exists(classPath.ClassDirectory))
            Directory.CreateDirectory(classPath.ClassDirectory);

        CreateFakerFile(srcDirectory, testDirectory, Dto.Creation, entity, projectBaseName);
        CreateFakerFile(srcDirectory, testDirectory, Dto.Update, entity, projectBaseName);
        CreateFakerFile(srcDirectory, testDirectory, EntityModel.Creation, entity, projectBaseName);
        CreateFakerFile(srcDirectory, testDirectory, EntityModel.Update, entity, projectBaseName);
    }

    public void CreateRolePermissionFakes(string srcDirectory, string solutionDirectory, string testDirectory, string projectBaseName, Entity entity)
    {
        var classPath = ClassPathHelper.TestFakesClassPath(testDirectory, $"", entity.Name, projectBaseName);

        if (!Directory.Exists(classPath.ClassDirectory))
            Directory.CreateDirectory(classPath.ClassDirectory);

        CreateRolePermissionFakerForCreationOrUpdateFile(srcDirectory, solutionDirectory, testDirectory, FileNames.GetDtoName(entity.Name, Dto.Creation), entity, projectBaseName);
        CreateRolePermissionFakerForCreationOrUpdateFile(srcDirectory, solutionDirectory, testDirectory, FileNames.GetDtoName(entity.Name, Dto.Update), entity, projectBaseName);
        CreateRolePermissionFakerForCreationOrUpdateFile(srcDirectory, solutionDirectory, testDirectory, EntityModel.Creation.GetClassName(entity.Name), entity, projectBaseName);
        CreateRolePermissionFakerForCreationOrUpdateFile(srcDirectory, solutionDirectory, testDirectory, EntityModel.Update.GetClassName(entity.Name), entity, projectBaseName);
    }
    
    public void CreateUserFakes(string srcDirectory, string solutionDirectory, string testDirectory, string projectBaseName, Entity entity)
    {
        var classPath = ClassPathHelper.TestFakesClassPath(testDirectory, $"", entity.Name, projectBaseName);

        if (!Directory.Exists(classPath.ClassDirectory))
            Directory.CreateDirectory(classPath.ClassDirectory);

        CreateUserFakerForCreationOrUpdateFile(srcDirectory, solutionDirectory, testDirectory, FileNames.GetDtoName(entity.Name, Dto.Creation), entity, projectBaseName);
        CreateUserFakerForCreationOrUpdateFile(srcDirectory, solutionDirectory, testDirectory, FileNames.GetDtoName(entity.Name, Dto.Update), entity, projectBaseName);
        CreateUserFakerForCreationOrUpdateFile(srcDirectory, solutionDirectory, testDirectory, EntityModel.Creation.GetClassName(entity.Name), entity, projectBaseName);
        CreateUserFakerForCreationOrUpdateFile(srcDirectory, solutionDirectory, testDirectory, EntityModel.Update.GetClassName(entity.Name), entity, projectBaseName);
    }

    public void CreateAddressFakes(string srcDirectory, string testDirectory, string projectBaseName)
    {
        var entity = new Entity();
        entity.Name = "Address";
        entity.Plural = "Addresses";
        var classPath = ClassPathHelper.TestFakesClassPath(testDirectory, $"", entity.Name, projectBaseName);

        if (!Directory.Exists(classPath.ClassDirectory))
            Directory.CreateDirectory(classPath.ClassDirectory);

        CreateAddressFakerForReadDtoFile(srcDirectory, testDirectory, projectBaseName);

        CreateAddressFakerForCreationOrUpdateFile(srcDirectory, testDirectory, FileNames.GetDtoName(entity.Name, Dto.Creation), entity, projectBaseName);
        CreateAddressFakerForCreationOrUpdateFile(srcDirectory, testDirectory, FileNames.GetDtoName(entity.Name, Dto.Update), entity, projectBaseName);
    }

    private void CreateFakerFile(string srcDirectory, string testDirectory, Dto dtoType, Entity entity, string projectBaseName)
    {
        var objectToFakeClassName = FileNames.GetDtoName(entity.Name, dtoType);
        var fakeFilename = $"Fake{objectToFakeClassName}.cs";
        var classPath = ClassPathHelper.TestFakesClassPath(testDirectory, fakeFilename, entity.Name, projectBaseName);
        var fileText = GetFakeFileText(classPath.ClassNamespace, dtoType, entity, srcDirectory, testDirectory, projectBaseName);
        _utilities.CreateFile(classPath, fileText);
    }

    private static string GetFakeFileText(string classNamespace, Dto dtoType, Entity entity, string srcDirectory, string testDirectory, string projectBaseName)
    {
        var objectToFakeClassName = FileNames.GetDtoName(entity.Name, dtoType);
        var entitiesClassPath = ClassPathHelper.EntityClassPath(testDirectory, "", entity.Plural, projectBaseName);
        var dtoClassPath = ClassPathHelper.DtoClassPath(srcDirectory, "", entity.Plural, projectBaseName);

        var rulesFor = "";
        foreach (var entityProperty in entity.Properties)
        {
            if (entityProperty.ValueObjectType.IsEmail)
            {
                rulesFor += @$"
        RuleFor({entity.Lambda} => {entity.Lambda}.{entityProperty.Name}, f => f.Person.Email);";
            }
            
            if(entityProperty.IsSmartEnum() && (dtoType is Dto.Creation or Dto.Update))
                rulesFor += @$"
        RuleFor({entity.Lambda} => {entity.Lambda}.{entityProperty.Name}, f => f.PickRandom<{entityProperty.SmartEnumPropName}>({entityProperty.SmartEnumPropName}.List).Name);";
        }

        // this... is super fragile. Should really refactor this
        var usingStatement = string.Empty;
        if (objectToFakeClassName.Contains("DTO", StringComparison.InvariantCultureIgnoreCase))
            usingStatement = @$"using {dtoClassPath.ClassNamespace};";

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

    private void CreateFakerFile(string srcDirectory, string testDirectory, EntityModel modelType, Entity entity, string projectBaseName)
    {
        var objectToFakeClassName = modelType.GetClassName(entity.Name);
        var fakeFilename = $"Fake{objectToFakeClassName}.cs";
        var classPath = ClassPathHelper.TestFakesClassPath(testDirectory, fakeFilename, entity.Name, projectBaseName);
        var fileText = GetFakeFileText(classPath.ClassNamespace, modelType, entity, srcDirectory, testDirectory, projectBaseName);
        _utilities.CreateFile(classPath, fileText);
    }
    
    private static string GetFakeFileText(string classNamespace, EntityModel modelType, Entity entity, string srcDirectory, string testDirectory, string projectBaseName)
    {
        var objectToFakeClassName = modelType.GetClassName(entity.Name);
        var entitiesClassPath = ClassPathHelper.EntityClassPath(testDirectory, "", entity.Plural, projectBaseName);
        var modelClassPath = ClassPathHelper.EntityModelClassPath(srcDirectory, entity.Name, entity.Plural, null, projectBaseName);

        var rulesFor = "";
        foreach (var entityProperty in entity.Properties)
        {
            if (entityProperty.ValueObjectType.IsEmail)
            {
                rulesFor += @$"
        RuleFor({entity.Lambda} => {entity.Lambda}.{entityProperty.Name}, f => f.Person.Email);";
            }
            
            if(entityProperty.IsSmartEnum())
                rulesFor += @$"
        RuleFor({entity.Lambda} => {entity.Lambda}.{entityProperty.Name}, f => f.PickRandom<{entityProperty.SmartEnumPropName}>({entityProperty.SmartEnumPropName}.List).Name);";
        }

        var usingStatement = @$"using {modelClassPath.ClassNamespace};";

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

    private void CreateRolePermissionFakerForCreationOrUpdateFile(string srcDirectory, string solutionDirectory, string testDirectory, string objectToFakeClassName, Entity entity, string projectBaseName)
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

        _utilities.CreateFile(classPath, fileText);
    }


    private void CreateUserFakerForCreationOrUpdateFile(string srcDirectory, string solutionDirectory, string testDirectory, string objectToFakeClassName, Entity entity, string projectBaseName)
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

        _utilities.CreateFile(classPath, fileText);
    }
    private void CreateAddressFakerForCreationOrUpdateFile(string srcDirectory, string testDirectory, string objectToFakeClassName, Entity entity, string projectBaseName)
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

        _utilities.CreateFile(classPath, fileText);
    }
    
    private void CreateAddressFakerForReadDtoFile(string srcDirectory, string testDirectory, string projectBaseName)
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

        _utilities.CreateFile(classPath, fileText);
    }
}
