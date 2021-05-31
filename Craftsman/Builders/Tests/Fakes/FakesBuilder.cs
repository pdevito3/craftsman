namespace Craftsman.Builders.Tests.Fakes
{
    using Craftsman.Enums;
    using Craftsman.Exceptions;
    using Craftsman.Helpers;
    using Craftsman.Models;
    using System;
    using System.IO;
    using System.Text;

    public static class FakesBuilder
    {
        public static void CreateFakes(string solutionDirectory, string solutionName, Entity entity)
        {
            // ****this class path will have an invalid FullClassPath. just need the directory
            var classPath = ClassPathHelper.TestFakesClassPath(solutionDirectory, $"", entity.Name, solutionName);

            if (!Directory.Exists(classPath.ClassDirectory))
                Directory.CreateDirectory(classPath.ClassDirectory);

            CreateFakerFile(solutionDirectory, entity.Name, entity, solutionName);
            CreateFakerFile(solutionDirectory, Utilities.GetDtoName(entity.Name, Dto.Creation), entity, solutionName);
            CreateFakerFile(solutionDirectory, Utilities.GetDtoName(entity.Name, Dto.Read), entity, solutionName);
            CreateFakerFile(solutionDirectory, Utilities.GetDtoName(entity.Name, Dto.Update), entity, solutionName);
        }

        private static void CreateFakerFile(string solutionDirectory, string objectToFakeClassName, Entity entity, string solutionName)
        {
            var fakeFilename = $"Fake{objectToFakeClassName}.cs";
            var classPath = ClassPathHelper.TestFakesClassPath(solutionDirectory, fakeFilename, entity.Name, solutionName);

            if (File.Exists(classPath.FullClassPath))
                throw new FileAlreadyExistsException(classPath.FullClassPath);

            using (FileStream fs = File.Create(classPath.FullClassPath))
            {
                var data = GetFakeFileText(classPath.ClassNamespace, objectToFakeClassName, entity, solutionDirectory, solutionName);
                fs.Write(Encoding.UTF8.GetBytes(data));
            }
        }

        private static string GetFakeFileText(string classNamespace, string objectToFakeClassName, Entity entity, string solutionDirectory, string projectBaseName)
        {
            var entitiesClassPath = ClassPathHelper.EntityClassPath(solutionDirectory, "", projectBaseName);
            var dtoClassPath = ClassPathHelper.DtoClassPath(solutionDirectory, "", entity.Name, projectBaseName);

            // this... is super fragile. Should really refactor this
            var usingStatement = objectToFakeClassName.Contains("DTO", StringComparison.InvariantCultureIgnoreCase) ? @$"using {dtoClassPath.ClassNamespace};" : $"using {entitiesClassPath.ClassNamespace};";
            return @$"namespace {classNamespace}
{{
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
    }}
}}";
        }
    }
}