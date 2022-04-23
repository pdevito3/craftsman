namespace Craftsman.Tests.ModelTests
{
    using Domain;
    using FluentAssertions;
    using System.IO;
    using Services;
    using Xunit;

    public class ClassPathTests
    {
        [Fact]
        public void ClassPathconstructorBuildsPropsCorrectly()
        {
            var dir = Path.Combine($"C:","repos");
            var topPath = Path.Combine($"Domain","Entities");
            var className = "class.cs";

            var classPath = new ClassPath(dir, topPath, className);

            classPath.ClassDirectory.Should().Be(Path.Combine(dir, topPath));
            classPath.FullClassPath.Should().Be(Path.Combine(classPath.ClassDirectory, className));
            classPath.ClassNamespace.Should().Be(topPath.Replace(Path.DirectorySeparatorChar, '.'));
        }
    }
}
