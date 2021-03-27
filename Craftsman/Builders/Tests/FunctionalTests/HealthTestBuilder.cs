namespace Craftsman.Builders.Tests.FunctionalTests
{
    using Craftsman.Enums;
    using Craftsman.Exceptions;
    using Craftsman.Helpers;
    using Craftsman.Models;
    using System;
    using System.Collections.Generic;
    using System.IO;
    using System.Linq;
    using System.Text;
    using static Helpers.ConsoleWriter;

    public class HealthTestBuilder
    {
        public static void CreateTests(string solutionDirectory, string projectBaseName)
        {
            try
            {
                var classPath = ClassPathHelper.FunctionalTestClassPath(solutionDirectory, $"HealthCheckTests.cs", "HealthChecks", projectBaseName);

                if (!Directory.Exists(classPath.ClassDirectory))
                    Directory.CreateDirectory(classPath.ClassDirectory);

                if (File.Exists(classPath.FullClassPath))
                    throw new FileAlreadyExistsException(classPath.FullClassPath);

                using (FileStream fs = File.Create(classPath.FullClassPath))
                {
                    var data = WriteTestFileText(solutionDirectory, classPath, projectBaseName);
                    fs.Write(Encoding.UTF8.GetBytes(data));
                }
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

        private static string WriteTestFileText(string solutionDirectory, ClassPath classPath, string projectBaseName)
        {
            var testUtilClassPath = ClassPathHelper.FunctionalTestUtilitiesClassPath(solutionDirectory, projectBaseName, "");

            return @$"namespace {classPath.ClassNamespace}
{{
    using {testUtilClassPath.ClassNamespace};
    using FluentAssertions;
    using NUnit.Framework;
    using System.Net.Http;
    using System.Threading.Tasks;

    public class {Path.GetFileNameWithoutExtension(classPath.FullClassPath)} : TestBase
    {{
        {HealthTest()}
    }}
}}";
        }

        private static string HealthTest()
        {
            return $@"[Test]
        public async Task Health_Check_Returns_Ok()
        {{
            // Arrange
            // N/A

            // Act
            var result = await _client.GetRequestAsync(ApiRoutes.Health);

            // Assert
            result.StatusCode.Should().Be(200);
        }}";
        }
    }
}
