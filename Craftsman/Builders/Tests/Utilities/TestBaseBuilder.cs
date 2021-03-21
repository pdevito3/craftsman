namespace Craftsman.Builders
{
    using Craftsman.Exceptions;
    using Craftsman.Helpers;
    using System;
    using System.IO.Abstractions;
    using System.Text;
    using static Helpers.ConsoleWriter;

    public class TestBaseBuilder
    {
        public static void CreateBase(string solutionDirectory, string projectBaseName, IFileSystem fileSystem)
        {
            try
            {
                var classPath = ClassPathHelper.IntegrationTestUtilitiesClassPath(solutionDirectory, projectBaseName, "TestBase.cs");

                if (!fileSystem.Directory.Exists(classPath.ClassDirectory))
                    fileSystem.Directory.CreateDirectory(classPath.ClassDirectory);

                if (fileSystem.File.Exists(classPath.FullClassPath))
                    throw new FileAlreadyExistsException(classPath.FullClassPath);

                using (var fs = fileSystem.File.Create(classPath.FullClassPath))
                {
                    var data = "";
                    data = GetBaseText(classPath.ClassNamespace, solutionDirectory, projectBaseName);
                    fs.Write(Encoding.UTF8.GetBytes(data));
                }

                GlobalSingleton.AddCreatedFile(classPath.FullClassPath.Replace($"{solutionDirectory}{fileSystem.Path.DirectorySeparatorChar}", ""));
            }
            catch (FileAlreadyExistsException e)
            {
                WriteError(e.Message);
                throw;
            }
            catch (Exception e)
            {
                WriteError($"An unhandled exception occurred when running the API command.\nThe error details are: \n{e.Message}");
                throw;
            }
        }

        public static string GetBaseText(string classNamespace, string solutionDirectory, string projectBaseName)
        {
            var testFixtureName = Utilities.GetIntegrationTestFixtureName();
            var integTestUtilsClassPath = ClassPathHelper.IntegrationTestUtilitiesClassPath(solutionDirectory, projectBaseName, testFixtureName);

            return @$"namespace {classNamespace}
{{
    using NUnit.Framework;
    using System.Threading.Tasks;
    using static {integTestUtilsClassPath.ClassNamespace}.{testFixtureName};

    public class TestBase
    {{
        [SetUp]
        public async Task TestSetUp()
        {{
            await ResetState();
        }}
    }}
}}";
        }
    }
}