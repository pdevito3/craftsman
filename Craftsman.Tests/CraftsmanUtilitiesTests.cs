namespace Craftsman.Tests;

using System;
using System.IO;
using System.IO.Abstractions;
using Craftsman.Exceptions;
using Craftsman.Helpers;
using FluentAssertions;
using Xunit;

// Uses the real file system: MockFileSystem matches `*.sln` against `.slnx` files like Windows does,
// which hides the macOS and Linux behavior that these tests guard.
public class CraftsmanUtilitiesTests : IDisposable
{
    private readonly string _directory = Directory.CreateTempSubdirectory("craftsman-tests-").FullName;
    private readonly CraftsmanUtilities _utilities = new(null!, new FileSystem(), null!, null!);

    public void Dispose() => Directory.Delete(_directory, true);

    [Theory]
    [InlineData("MyDomain.sln")]
    [InlineData("MyDomain.slnx")]
    [InlineData("MyDomain.SLNX")]
    public void IsSolutionDirectoryGuard_accepts_sln_and_slnx_files(string solutionFileName)
    {
        File.WriteAllText(Path.Combine(_directory, solutionFileName), string.Empty);

        var act = () => _utilities.IsSolutionDirectoryGuard(_directory);

        act.Should().NotThrow();
    }

    [Theory]
    [InlineData("MyDomain.sln.DotSettings.user")]
    [InlineData("README.md")]
    public void IsSolutionDirectoryGuard_throws_when_directory_has_no_solution_file(string otherFileName)
    {
        File.WriteAllText(Path.Combine(_directory, otherFileName), string.Empty);

        var act = () => _utilities.IsSolutionDirectoryGuard(_directory);

        act.Should().Throw<Exception>().WithMessage("A solution file was not found*")
            .Which.Should().BeAssignableTo<ICraftsmanException>();
    }
}
