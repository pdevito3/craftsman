namespace Craftsman.Builders.Tests.Fakes;

using System;
using System.IO;
using Domain;
using Domain.Enums;
using Helpers;
using Services;

public class FakesBuilder
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

        CreateFakerEntityFile(srcDirectory, testDirectory, entity.Name, entity, projectBaseName);
        CreateFakerFile(srcDirectory, testDirectory, Dto.Creation, entity, projectBaseName);
        CreateFakerFile(srcDirectory, testDirectory, Dto.Update, entity, projectBaseName);
    }

    public void CreateRolePermissionFakes(string srcDirectory, string solutionDirectory, string testDirectory, string projectBaseName, Entity entity)
    {
        var classPath = ClassPathHelper.TestFakesClassPath(testDirectory, $"", entity.Name, projectBaseName);

        if (!Directory.Exists(classPath.ClassDirectory))
            Directory.CreateDirectory(classPath.ClassDirectory);

        CreateFakerEntityFile(srcDirectory, testDirectory, entity.Name, entity, projectBaseName);

        CreateRolePermissionFakerForCreationOrUpdateFile(srcDirectory, solutionDirectory, testDirectory, FileNames.GetDtoName(entity.Name, Dto.Creation), entity, projectBaseName);
        CreateRolePermissionFakerForCreationOrUpdateFile(srcDirectory, solutionDirectory, testDirectory, FileNames.GetDtoName(entity.Name, Dto.Update), entity, projectBaseName);
    }

    public void CreateUserFakes(string srcDirectory, string solutionDirectory, string testDirectory, string projectBaseName, Entity entity)
    {
        var classPath = ClassPathHelper.TestFakesClassPath(testDirectory, $"", entity.Name, projectBaseName);

        if (!Directory.Exists(classPath.ClassDirectory))
            Directory.CreateDirectory(classPath.ClassDirectory);

        CreateFakerEntityFile(srcDirectory, testDirectory, entity.Name, entity, projectBaseName);

        CreateUserFakerForCreationOrUpdateFile(srcDirectory, solutionDirectory, testDirectory, FileNames.GetDtoName(entity.Name, Dto.Creation), entity, projectBaseName);
        CreateUserFakerForCreationOrUpdateFile(srcDirectory, solutionDirectory, testDirectory, FileNames.GetDtoName(entity.Name, Dto.Update), entity, projectBaseName);
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
public class Fake{objectToFakeClassName} : AutoFaker<{objectToFakeClassName}>
{{
    public Fake{objectToFakeClassName}()
    {{
        // if you want default values on any of your properties (e.g. an int between a certain range or a date always in the past), you can add `RuleFor` lines describing those defaults
        //RuleFor({entity.Lambda} => {entity.Lambda}.ExampleIntProperty, {entity.Lambda} => {entity.Lambda}.Random.Number(50, 100000));
        //RuleFor({entity.Lambda} => {entity.Lambda}.ExampleDateProperty, {entity.Lambda} => {entity.Lambda}.Date.Past());{rulesFor}
    }}
}}";
    }

    private void CreateFakerEntityFile(string srcDirectory, string testDirectory, string objectToFakeClassName, Entity entity, string projectBaseName)
    {
        var fakeFilename = $"Fake{objectToFakeClassName}.cs";
        var classPath = ClassPathHelper.TestFakesClassPath(testDirectory, fakeFilename, entity.Name, projectBaseName);
        var fileText = GetFakeEntityFileText(classPath.ClassNamespace, objectToFakeClassName, entity, srcDirectory, testDirectory, projectBaseName);
        _utilities.CreateFile(classPath, fileText);
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

public class Fake{objectToFakeClassName}
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

public class Fake{objectToFakeClassName} : AutoFaker<{objectToFakeClassName}>
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

        var fileText = @$"namespace {classPath.ClassNamespace};

using AutoBogus;
using {policyDomainClassPath.ClassNamespace};
using {dtoClassPath.ClassNamespace};
using {rolesClassPath.ClassNamespace};

public class Fake{objectToFakeClassName} : AutoFaker<{objectToFakeClassName}>
{{
    public Fake{objectToFakeClassName}()
    {{
        RuleFor(u => u.Email, f => f.Person.Email);
    }}
}}";

        _utilities.CreateFile(classPath, fileText);
    }
}
