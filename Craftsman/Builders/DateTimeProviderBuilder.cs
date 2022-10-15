namespace Craftsman.Builders;

using Helpers;
using Services;

public class DateTimeProviderBuilder
{
    private readonly ICraftsmanUtilities _utilities;

    public DateTimeProviderBuilder(ICraftsmanUtilities utilities)
    {
        _utilities = utilities;
    }
    public void GetCurrentUserService(string srcDirectory, string projectBaseName)
    {
        var classPath = ClassPathHelper.WebApiServicesClassPath(srcDirectory, "DateTimeProvider.cs", projectBaseName);
        var fileText = GetFileText(classPath.ClassNamespace);
        _utilities.CreateFile(classPath, fileText);
    }

    private static string GetFileText(string classNamespace)
    {
        return @$"namespace {classNamespace};

public interface IDateTimeProvider
{{
    DateTime DateTimeUtcNow {{ get; }}
    DateOnly DateOnlyUtcNow {{ get; }}
}}

public class DateTimeProvider : IDateTimeProvider
{{
    public DateTime DateTimeUtcNow => DateTime.UtcNow;
    public DateOnly DateOnlyUtcNow => DateOnly.FromDateTime(DateTimeUtcNow);
}}";
    }
}
