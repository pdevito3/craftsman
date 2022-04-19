namespace NewCraftsman.Builders.Tests.Fakes
{
    using System;
    using System.IO;
    using System.IO.Abstractions;

    public static class FakesBuilder
    {
        public static void CreateFakes(string solutionDirectory, string testDirectory, string projectBaseName, Entity entity, IFileSystem fileSystem)
        {
            // ****this class path will have an invalid FullClassPath. just need the directory
            var classPath = ClassPathHelper.TestFakesClassPath(testDirectory, $"", entity.Name, projectBaseName);

            if (!Directory.Exists(classPath.ClassDirectory))
                Directory.CreateDirectory(classPath.ClassDirectory);

            CreateFakerEntityFile(solutionDirectory, testDirectory, entity.Name, entity, projectBaseName, fileSystem);
            CreateFakerFile(solutionDirectory, testDirectory, FileNames.GetDtoName(entity.Name, Dto.Creation), entity, projectBaseName, fileSystem);
            CreateFakerFile(solutionDirectory, testDirectory, FileNames.GetDtoName(entity.Name, Dto.Read), entity, projectBaseName, fileSystem);
            CreateFakerFile(solutionDirectory, testDirectory, FileNames.GetDtoName(entity.Name, Dto.Update), entity, projectBaseName, fileSystem);
        }
        
        public static void CreateRolePermissionFakes(string solutionDirectory, string testDirectory, string projectBaseName, Entity entity, IFileSystem fileSystem)
        {
            var classPath = ClassPathHelper.TestFakesClassPath(testDirectory, $"", entity.Name, projectBaseName);

            if (!Directory.Exists(classPath.ClassDirectory))
                Directory.CreateDirectory(classPath.ClassDirectory);

            CreateFakerEntityFile(solutionDirectory, testDirectory, entity.Name, entity, projectBaseName, fileSystem);
            CreateFakerFile(solutionDirectory, testDirectory, FileNames.GetDtoName(entity.Name, Dto.Read), entity, projectBaseName, fileSystem);
            
            CreateRolePermissionFakerForCreationOrUpdateFile(solutionDirectory, testDirectory, FileNames.GetDtoName(entity.Name, Dto.Creation), entity, projectBaseName, fileSystem);
            CreateRolePermissionFakerForCreationOrUpdateFile(solutionDirectory, testDirectory, FileNames.GetDtoName(entity.Name, Dto.Update), entity, projectBaseName, fileSystem);
        }

        private static void CreateFakerFile(string solutionDirectory, string testDirectory, string objectToFakeClassName, Entity entity, string projectBaseName, IFileSystem fileSystem)
        {
            var fakeFilename = $"Fake{objectToFakeClassName}.cs";
            var classPath = ClassPathHelper.TestFakesClassPath(testDirectory, fakeFilename, entity.Name, projectBaseName);
            var fileText = GetFakeFileText(classPath.ClassNamespace, objectToFakeClassName, entity, solutionDirectory, testDirectory, projectBaseName);
            _utilities.CreateFile(classPath, fileText);
        }

        private static string GetFakeFileText(string classNamespace, string objectToFakeClassName, Entity entity, string solutionDirectory, string testDirectory, string projectBaseName)
        {
            var entitiesClassPath = ClassPathHelper.EntityClassPath(testDirectory, "", entity.Plural, projectBaseName);
            var dtoClassPath = ClassPathHelper.DtoClassPath(solutionDirectory, "", entity.Name, projectBaseName);

            // this... is super fragile. Should really refactor this
            var usingStatement = objectToFakeClassName.Contains("DTO", StringComparison.InvariantCultureIgnoreCase) ? @$"using {dtoClassPath.ClassNamespace};" : $"using {entitiesClassPath.ClassNamespace};";
            return @$"namespace {classNamespace};

using AutoBogus;
{usingStatement}

// or replace 'AutoFaker' with 'Faker' along with your own rules if you don't want all fields to be auto faked
public class Fake{objectToFakeClassName} : AutoFaker<{objectToFakeClassName}>
{{
    public Fake{objectToFakeClassName}()
    {{
        // if you want default values on any of your properties (e.g. an int between a certain range or a date always in the past), you can add `RuleFor` lines describing those defaults
        //RuleFor({entity.Lambda} => {entity.Lambda}.ExampleIntProperty, {entity.Lambda} => {entity.Lambda}.Random.Number(50, 100000));
        //RuleFor({entity.Lambda} => {entity.Lambda}.ExampleDateProperty, {entity.Lambda} => {entity.Lambda}.Date.Past());
    }}
}}";
        }

        private static void CreateFakerEntityFile(string solutionDirectory, string testDirectory, string objectToFakeClassName, Entity entity, string projectBaseName, IFileSystem fileSystem)
        {
            var fakeFilename = $"Fake{objectToFakeClassName}.cs";
            var classPath = ClassPathHelper.TestFakesClassPath(testDirectory, fakeFilename, entity.Name, projectBaseName);
            var fileText = GetFakeEntityFileText(classPath.ClassNamespace, objectToFakeClassName, entity, solutionDirectory, testDirectory, projectBaseName);
            _utilities.CreateFile(classPath, fileText);
        }

        private static string GetFakeEntityFileText(string classNamespace, string objectToFakeClassName, Entity entity, string solutionDirectory, string testDirectory, string projectBaseName)
        {
            var entitiesClassPath = ClassPathHelper.EntityClassPath(testDirectory, "", entity.Plural, projectBaseName);
            var dtoClassPath = ClassPathHelper.DtoClassPath(solutionDirectory, "", entity.Name, projectBaseName);
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
        return {entity.Name}.Create(new {fakeCreationDtoName}().Generate());
    }}
}}";
        }
        

        private static void CreateRolePermissionFakerForCreationOrUpdateFile(string solutionDirectory, string testDirectory, string objectToFakeClassName, Entity entity, string projectBaseName, IFileSystem fileSystem)
        {
            var fakeFilename = $"Fake{objectToFakeClassName}.cs";
            var classPath = ClassPathHelper.TestFakesClassPath(testDirectory, fakeFilename, entity.Name, projectBaseName);
            
            var dtoClassPath = ClassPathHelper.DtoClassPath(solutionDirectory, "", entity.Name, projectBaseName);
            var policyDomainClassPath = ClassPathHelper.PolicyDomainClassPath(testDirectory, "",projectBaseName);
            var rolesClassPath = ClassPathHelper.SharedKernelDomainClassPath(solutionDirectory, "");

            var fileText= @$"namespace {classPath.ClassNamespace};

using AutoBogus;
using {policyDomainClassPath.ClassNamespace};
using {dtoClassPath.ClassNamespace};
using {rolesClassPath.ClassNamespace};

public class Fake{objectToFakeClassName} : AutoFaker<{objectToFakeClassName}>
{{
    public Fake{objectToFakeClassName}()
    {{
        RuleFor(rp => rp.Permission, f => f.PickRandom(Permissions.List()));
        RuleFor(rp => rp.Role, f => f.PickRandom(Roles.List()));
    }}
}}";

            _utilities.CreateFile(classPath, fileText);
        }
    }
}