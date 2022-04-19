namespace NewCraftsman.Builders.Tests.UnitTests
{
    using System.IO;
    using System.Text;

    public class PagedListTestBuilder
    {
        public static void CreateTests(string srcDirectory, string testDirectory, string projectBaseName)
        {
            var classPath = ClassPathHelper.UnitTestWrapperTestsClassPath(testDirectory, $"PagedListTests.cs", projectBaseName);

            if (!Directory.Exists(classPath.ClassDirectory))
                Directory.CreateDirectory(classPath.ClassDirectory);

            if (File.Exists(classPath.FullClassPath))
                throw new FileAlreadyExistsException(classPath.FullClassPath);

            using (FileStream fs = File.Create(classPath.FullClassPath))
            {
                var data = WriteTestFileText(srcDirectory, classPath, projectBaseName);
                fs.Write(Encoding.UTF8.GetBytes(data));
            }
        }

        private static string WriteTestFileText(string srcDirectory, ClassPath classPath, string projectBaseName)
        {
            var wrapperClassPath = ClassPathHelper.WrappersClassPath(srcDirectory, "", projectBaseName);

            return @$"namespace {classPath.ClassNamespace};

using {wrapperClassPath.ClassNamespace};
using FluentAssertions;
using NUnit.Framework;

public class {Path.GetFileNameWithoutExtension(classPath.FullClassPath)}
{{
    [Test]
    public void pagedlist_returns_accurate_data_for_standard_pagination()
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
    public void pagedlist_returns_accurate_data_with_last_record()
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
}}";
        }
    }
}