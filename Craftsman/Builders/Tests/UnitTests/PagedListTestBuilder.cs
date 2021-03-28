namespace Craftsman.Builders.Tests.UnitTests
{
    using Craftsman.Enums;
    using Craftsman.Exceptions;
    using Craftsman.Helpers;
    using Craftsman.Models;
    using System;
    using System.Collections.Generic;
    using System.IO;
    using System.Text;
    using static Helpers.ConsoleWriter;

    public class PagedListTestBuilder
    {
        public static void CreateTests(string solutionDirectory, string projectBaseName)
        {
            try
            {
                var classPath = ClassPathHelper.UnitTestWrapperTestsClassPath(solutionDirectory, $"PagedListTests.cs", projectBaseName);

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
            var wrapperClassPath = ClassPathHelper.WrappersClassPath(solutionDirectory, "", projectBaseName);


            return @$"namespace {classPath.ClassNamespace}
{{
    using {wrapperClassPath.ClassNamespace};
    using FluentAssertions;
    using NUnit.Framework;
    using System.Collections.Generic;
    using System.Linq;

    public class {Path.GetFileNameWithoutExtension(classPath.FullClassPath)} : TestBase
    {{
        [Test]
        public void PagedList_Returns_Accurate_Data_For_Standard_Pagination()
        {{
            var pageNumber = 2;
            var pageSize = 2;
            var source = new List<int>() {{ 1, 2, 3, 4, 5 }}.AsQueryable();

            var list = PagedList<int>.Create(source, pageNumber, pageSize);
            list.TotalCount.Should().Be(5);
            list.PageSize.Should().Be(2);
            list.PageNumber.Should().Be(2);
            list.CurrentPageSize.Should().Be(2);
            list.CurrentStartIndex.Should().Be(3);
            list.CurrentEndIndex.Should().Be(4);
            list.TotalPages.Should().Be(3);
        }}

        [Test]
        public void PagedList_Returns_Accurate_Data_With_Last_Record()
        {{
            var pageNumber = 3;
            var pageSize = 2;
            var source = new List<int>() {{ 1, 2, 3, 4, 5 }}.AsQueryable();

            var list = PagedList<int>.Create(source, pageNumber, pageSize);
            list.TotalCount.Should().Be(5);
            list.PageSize.Should().Be(2);
            list.PageNumber.Should().Be(3);
            list.CurrentPageSize.Should().Be(1);
            list.CurrentStartIndex.Should().Be(5);
            list.CurrentEndIndex.Should().Be(5);
            list.TotalPages.Should().Be(3);
        }}
    }}
}}";
        }
    }
}
