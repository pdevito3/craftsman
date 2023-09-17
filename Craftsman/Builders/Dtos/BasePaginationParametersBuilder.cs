namespace Craftsman.Builders.Dtos;

using Helpers;
using Services;

public class BasePaginationParametersBuilder
{
    private readonly ICraftsmanUtilities _utilities;

    public BasePaginationParametersBuilder(ICraftsmanUtilities utilities)
    {
        _utilities = utilities;
    }

    public void CreateBasePaginationParameters(string srcDirectory, string projectBaseName)
    {
        var classPath = ClassPathHelper.WebApiResourcesClassPath(srcDirectory, $"BasePaginationParameters.cs", projectBaseName);
        var fileText = GetBasePaginationParametersText(classPath.ClassNamespace);
        _utilities.CreateFile(classPath, fileText);
    }

    public static string GetBasePaginationParametersText(string classNamespace)
    {
        return @$"namespace {classNamespace};
public abstract class BasePaginationParameters
{{
    internal virtual int MaxPageSize {{ get; }} = 500;
    internal virtual int DefaultPageSize {{ get; set; }} = 10;

    public virtual int PageNumber {{ get; set; }} = 1;

    public int PageSize
    {{
        get
        {{
            return DefaultPageSize;
        }}
        set
        {{
            DefaultPageSize = value > MaxPageSize ? MaxPageSize : value;
        }}
    }}
}}";
    }
}
