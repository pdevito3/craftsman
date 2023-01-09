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

    public void CreateFakes(string srcDirectory, string testDirectory, string projectBaseName, Entity entity, bool overwrite)
    {
        // ****this class path will have an invalid FullClassPath. just need the directory
        var classPath = ClassPathHelper.TestFakesClassPath(testDirectory, $"", entity.Name, projectBaseName);

        if (!Directory.Exists(classPath.ClassDirectory))
            Directory.CreateDirectory(classPath.ClassDirectory);

        CreateFakerEntityFile(srcDirectory, testDirectory, entity.Name, entity, projectBaseName, overwrite);
        CreateFakerFile(srcDirectory, testDirectory, Dto.Creation, entity, projectBaseName, overwrite);
        CreateFakerFile(srcDirectory, testDirectory, Dto.Update, entity, projectBaseName, overwrite);
    }

    public void CreateRolePermissionFakes(string srcDirectory, string solutionDirectory, string testDirectory, string projectBaseName, Entity entity, bool overwrite)
    {
        var classPath = ClassPathHelper.TestFakesClassPath(testDirectory, $"", entity.Name, projectBaseName);

        if (!Directory.Exists(classPath.ClassDirectory))
            Directory.CreateDirectory(classPath.ClassDirectory);

        CreateFakerEntityFile(srcDirectory, testDirectory, entity.Name, entity, projectBaseName, overwrite);

        CreateRolePermissionFakerForCreationOrUpdateFile(srcDirectory, solutionDirectory, testDirectory, FileNames.GetDtoName(entity.Name, Dto.Creation), entity, projectBaseName);
        CreateRolePermissionFakerForCreationOrUpdateFile(srcDirectory, solutionDirectory, testDirectory, FileNames.GetDtoName(entity.Name, Dto.Update), entity, projectBaseName);
    }

    public void CreateAddressFakes(string srcDirectory, string testDirectory, string projectBaseName, bool overwrite)
    {
        var entity = new Entity();
        entity.Name = "Address";
        entity.Plural = "Addresses";
        var classPath = ClassPathHelper.TestFakesClassPath(testDirectory, $"", entity.Name, projectBaseName);

        if (!Directory.Exists(classPath.ClassDirectory))
            Directory.CreateDirectory(classPath.ClassDirectory);

        CreateAddressFakerForReadDtoFile(srcDirectory, testDirectory, projectBaseName, overwrite);

        CreateAddressFakerForCreationOrUpdateFile(srcDirectory, testDirectory, FileNames.GetDtoName(entity.Name, Dto.Creation), entity, projectBaseName, overwrite);
        CreateAddressFakerForCreationOrUpdateFile(srcDirectory, testDirectory, FileNames.GetDtoName(entity.Name, Dto.Update), entity, projectBaseName, overwrite);
    }

    public void CreateUserFakes(string srcDirectory, string solutionDirectory, string testDirectory, string projectBaseName, Entity entity, bool overwrite)
    {
        var classPath = ClassPathHelper.TestFakesClassPath(testDirectory, $"", entity.Name, projectBaseName);

        if (!Directory.Exists(classPath.ClassDirectory))
            Directory.CreateDirectory(classPath.ClassDirectory);

        CreateFakerEntityFile(srcDirectory, testDirectory, entity.Name, entity, projectBaseName, overwrite);

        CreateUserFakerForCreationOrUpdateFile(srcDirectory, solutionDirectory, testDirectory, FileNames.GetDtoName(entity.Name, Dto.Creation), entity, projectBaseName, overwrite);
        CreateUserFakerForCreationOrUpdateFile(srcDirectory, solutionDirectory, testDirectory, FileNames.GetDtoName(entity.Name, Dto.Update), entity, projectBaseName, overwrite);
    }

    private void CreateFakerFile(string srcDirectory, string testDirectory, Dto dtoType, Entity entity, string projectBaseName, bool overwrite = false)
    {
        var objectToFakeClassName = FileNames.GetDtoName(entity.Name, dtoType);
        var fakeFilename = $"Fake{objectToFakeClassName}.cs";
        var classPath = ClassPathHelper.TestFakesClassPath(testDirectory, fakeFilename, entity.Name, projectBaseName);
        var fileText = GetFakeFileText(classPath.ClassNamespace, dtoType, entity, srcDirectory, testDirectory, projectBaseName);
        _utilities.CreateFile(classPath, fileText, overwrite);
    }

    private static string GetFakeFileText(string classNamespace, Dto dtoType, Entity entity, string srcDirectory, string testDirectory, string projectBaseName)
    {
        var objectToFakeClassName = FileNames.GetDtoName(entity.Name, dtoType);
        var entitiesClassPath = ClassPathHelper.EntityClassPath(testDirectory, "", entity.Plural, projectBaseName);
        var dtoClassPath = ClassPathHelper.DtoClassPath(srcDirectory, "", entity.Plural, projectBaseName);

        var rulesFor = "";
        foreach (var entityProperty in entity.Properties)
        {
            if(entityProperty.IsSmartEnum() && (dtoType is Dto.Creation or Dto.Update))
                rulesFor += @$"
        RuleFor({entity.Lambda} => {entity.Lambda}.{entityProperty.Name}, f => f.PickRandom<{entityProperty.SmartEnumPropName}>({entityProperty.SmartEnumPropName}.List).Name);";
        }

        // this... is super fragile. Should really refactor this
        var usingStatement = objectToFakeClassName.Contains("DTO", StringComparison.InvariantCultureIgnoreCase) ? @$"using {dtoClassPath.ClassNamespace};" : $"";
        return @$"namespace {classNamespace};

using AutoBogus;
using {entitiesClassPath.ClassNamespace};
{usingStatement}

// or replace 'AutoFaker' with 'Faker' along with your own rules if you don't want all fields to be auto faked
public sealed class Fake{objectToFakeClassName} : AutoFaker<{objectToFakeClassName}>
{{
    public Fake{objectToFakeClassName}()
    {{
        // if you want default values on any of your properties (e.g. an int between a certain range or a date always in the past), you can add `RuleFor` lines describing those defaults
        //RuleFor({entity.Lambda} => {entity.Lambda}.ExampleIntProperty, {entity.Lambda} => {entity.Lambda}.Random.Number(50, 100000));
        //RuleFor({entity.Lambda} => {entity.Lambda}.ExampleDateProperty, {entity.Lambda} => {entity.Lambda}.Date.Past());{rulesFor}
    }}
}}";
    }

    private void CreateFakerEntityFile(string srcDirectory, string testDirectory, string objectToFakeClassName, Entity entity, string projectBaseName, bool overwrite)
    {
        var fakeFilename = $"Fake{objectToFakeClassName}.cs";
        var classPath = ClassPathHelper.TestFakesClassPath(testDirectory, fakeFilename, entity.Name, projectBaseName);
        var fileText = GetFakeEntityFileText(classPath.ClassNamespace, objectToFakeClassName, entity, srcDirectory, testDirectory, projectBaseName);
        _utilities.CreateFile(classPath, fileText, overwrite);
    }

    private static string GetFakeEntityFileText(string classNamespace, string objectToFakeClassName, Entity entity, string srcDirectory, string testDirectory, string projectBaseName)
    {
        var entitiesClassPath = ClassPathHelper.EntityClassPath(testDirectory, "", entity.Plural, projectBaseName);
        var dtoClassPath = ClassPathHelper.DtoClassPath(srcDirectory, "", entity.Plural, projectBaseName);
        var creationDtoName = FileNames.GetDtoName(entity.Name, Dto.Creation);
        var fakeCreationDtoName = $"Fake{creationDtoName}";

        return @$"namespace {classNamespace};

using AutoBogus;
using {entitiesClassPath.ClassNamespace};
using {dtoClassPath.ClassNamespace};

public sealed class Fake{objectToFakeClassName}
{{
    public static {entity.Name} Generate({creationDtoName} {creationDtoName.LowercaseFirstLetter()})
    {{
        return {entity.Name}.Create({creationDtoName.LowercaseFirstLetter()});
    }}

    public static {entity.Name} Generate()
    {{
        return Generate(new {fakeCreationDtoName}().Generate());
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

        var fileText = @$"namespace {classPath.ClassNamespace};

using AutoBogus;
using {policyDomainClassPath.ClassNamespace};
using {dtoClassPath.ClassNamespace};
using {rolesClassPath.ClassNamespace};

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


    private void CreateUserFakerForCreationOrUpdateFile(string srcDirectory, string solutionDirectory, string testDirectory, string objectToFakeClassName, Entity entity, string projectBaseName, bool overwrite)
    {
        var fakeFilename = $"Fake{objectToFakeClassName}.cs";
        var classPath = ClassPathHelper.TestFakesClassPath(testDirectory, fakeFilename, entity.Name, projectBaseName);

        var dtoClassPath = ClassPathHelper.DtoClassPath(srcDirectory, "", entity.Plural, projectBaseName);
        var policyDomainClassPath = ClassPathHelper.PolicyDomainClassPath(testDirectory, "", projectBaseName);
        var rolesClassPath = ClassPathHelper.EntityClassPath(solutionDirectory, "", "Roles", projectBaseName);

        var fileText = @$"namespace {classPath.ClassNamespace};

using AutoBogus;
using {policyDomainClassPath.ClassNamespace};
using {dtoClassPath.ClassNamespace};
using {rolesClassPath.ClassNamespace};

public sealed class Fake{objectToFakeClassName} : AutoFaker<{objectToFakeClassName}>
{{
    public Fake{objectToFakeClassName}()
    {{
        RuleFor(u => u.Email, f => f.Person.Email);
    }}
}}";

        _utilities.CreateFile(classPath, fileText, overwrite);
    }
    private void CreateAddressFakerForCreationOrUpdateFile(string srcDirectory, string testDirectory, string objectToFakeClassName, Entity entity, string projectBaseName, bool overwrite)
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

        _utilities.CreateFile(classPath, fileText, overwrite);
    }
    
    private void CreateAddressFakerForReadDtoFile(string srcDirectory, string testDirectory, string projectBaseName, bool overwrite)
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

        _utilities.CreateFile(classPath, fileText, overwrite);
    }
}
