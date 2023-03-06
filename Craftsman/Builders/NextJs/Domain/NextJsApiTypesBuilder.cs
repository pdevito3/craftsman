namespace Craftsman.Builders.NextJs.Domain;

using Craftsman.Domain;
using Craftsman.Domain.Enums;
using Craftsman.Helpers;
using Craftsman.Services;

public class NextJsApiTypesBuilder
{
    private readonly ICraftsmanUtilities _utilities;

    public NextJsApiTypesBuilder(ICraftsmanUtilities utilities)
    {
        _utilities = utilities;
    }

    public void CreateDynamicFeatureTypes(string spaDirectory, string entityName, string entityPlural, List<NextJsEntityProperty> props)
    {
        var routesIndexClassPath = ClassPathHelper.NextJsSpaFeatureClassPath(spaDirectory,
            entityPlural,
            NextJsDomainCategory.Types,
            "index.ts");
        var routesIndexFileText = GetFileText(entityName, props);
        _utilities.CreateFile(routesIndexClassPath, routesIndexFileText);
    }

    public static string GetFileText(string entityName, List<NextJsEntityProperty> props)
    {
        var readDtoName = FileNames.GetDtoName(entityName, Dto.Read);
        var dtoForCreationName = FileNames.GetDtoName(entityName, Dto.Creation);
        var dtoForUpdateName = FileNames.GetDtoName(entityName, Dto.Update);

        var propList = TypePropsBuilder(props);

        return @$"import {{ SortingState }} from ""@tanstack/react-table"";

export interface QueryParams {{
  pageNumber?: number;
  pageSize?: number;
  filters?: string;
  sortOrder?: SortingState;
}}

export interface {readDtoName} {{
  id: string;{propList}
}}

export interface {dtoForCreationName} {{{propList}
}}
export interface {dtoForUpdateName} {{{propList}
}}

// need a string enum list?
// const StatusList = ['Status1', 'Status2', null] as const;
// export type Status = typeof StatusList[number];
// Then use as --> status: Status;
";
    }

    private static string TypePropsBuilder(List<NextJsEntityProperty> props)
    {
        var propString = "";
        foreach (var bffEntityProperty in props)
        {
            var questionMark = bffEntityProperty.Nullable
                ? "?"
                : "";
            propString += $@"{Environment.NewLine}  {bffEntityProperty.Name.LowercaseFirstLetter()}{questionMark}: {bffEntityProperty.RawType};";
        }

        return propString;
    }


}
