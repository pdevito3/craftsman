namespace Craftsman.Tests.ModelTests
{
    using Craftsman.Models;
    using Craftsman.Tests.Fakes;
    using FluentAssertions;
    using NuGet.Frameworks;
    using System.Collections.Generic;
    using System.IO;
    using Xunit;

    public class ClassPathTests
    {
        [Fact]
        public void ClassPathconstructorBuildsPropsCorrectly()
        {
            var dir = $"C:\\repos";
            var topPath = $"Domain\\Entities";
            var className = "class.cs";

            var classPath = new ClassPath(dir, topPath, className);

            classPath.ClassDirectory.Should().Be(Path.Combine(dir, topPath));
            classPath.FullClassPath.Should().Be(Path.Combine(classPath.ClassDirectory, className));
            classPath.ClassNamespace.Should().Be(topPath.Replace("\\", "."));
        }
    }
}
