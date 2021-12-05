namespace Craftsman.Builders.Tests.Fakes
{
    using Craftsman.Enums;
    using Craftsman.Exceptions;
    using Craftsman.Helpers;
    using Craftsman.Models;
    using System;
    using System.IO;
    using System.IO.Abstractions;
    using System.Text;

    public static class FakesBuilder
    {
        public static void CreateFakes(string solutionDirectory, string projectName, Entity entity, IFileSystem fileSystem)
        {
            // ****this class path will have an invalid FullClassPath. just need the directory
            var classPath = ClassPathHelper.TestFakesClassPath(solutionDirectory, $"", entity.Name, projectName);

            if (!Directory.Exists(classPath.ClassDirectory))
                Directory.CreateDirectory(classPath.ClassDirectory);

            CreateFakerEntityFile(solutionDirectory, entity.Name, entity, projectName, fileSystem);
            CreateFakerFile(solutionDirectory, Utilities.GetDtoName(entity.Name, Dto.Creation), entity, projectName, fileSystem);
            CreateFakerFile(solutionDirectory, Utilities.GetDtoName(entity.Name, Dto.Read), entity, projectName, fileSystem);
            CreateFakerFile(solutionDirectory, Utilities.GetDtoName(entity.Name, Dto.Update), entity, projectName, fileSystem);
        }

        private static void CreateFakerFile(string solutionDirectory, string objectToFakeClassName, Entity entity, string projectName, IFileSystem fileSystem)
        {
            var fakeFilename = $"Fake{objectToFakeClassName}.cs";
            var classPath = ClassPathHelper.TestFakesClassPath(solutionDirectory, fakeFilename, entity.Name, projectName);
            var fileText = GetFakeFileText(classPath.ClassNamespace, objectToFakeClassName, entity, solutionDirectory, projectName);
            Utilities.CreateFile(classPath, fileText, fileSystem);
        }

        private static string GetFakeFileText(string classNamespace, string objectToFakeClassName, Entity entity, string solutionDirectory, string projectBaseName)
        {
            var entitiesClassPath = ClassPathHelper.EntityClassPath(solutionDirectory, "", entity.Plural, projectBaseName);
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

        private static void CreateFakerEntityFile(string solutionDirectory, string objectToFakeClassName, Entity entity, string projectName, IFileSystem fileSystem)
        {
            var fakeFilename = $"Fake{objectToFakeClassName}.cs";
            var classPath = ClassPathHelper.TestFakesClassPath(solutionDirectory, fakeFilename, entity.Name, projectName);
            var fileText = GetFakeEntityFileText(classPath.ClassNamespace, objectToFakeClassName, entity, solutionDirectory, projectName);
            Utilities.CreateFile(classPath, fileText, fileSystem);
        }

        private static string GetFakeEntityFileText(string classNamespace, string objectToFakeClassName, Entity entity, string solutionDirectory, string projectBaseName)
        {
            var entitiesClassPath = ClassPathHelper.EntityClassPath(solutionDirectory, "", entity.Plural, projectBaseName);
            var dtoClassPath = ClassPathHelper.DtoClassPath(solutionDirectory, "", entity.Name, projectBaseName);
            var creationDtoName = Utilities.GetDtoName(entity.Name, Dto.Creation);

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
}}";
        }
        
        
    }
}