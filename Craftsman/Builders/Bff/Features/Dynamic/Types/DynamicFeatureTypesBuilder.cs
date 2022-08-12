namespace Craftsman.Builders.Bff.Features.Dynamic.Types;

using Domain;
using Domain.Enums;
using Helpers;
using Services;

public class DynamicFeatureTypesBuilder
{
    private readonly ICraftsmanUtilities _utilities;

    public DynamicFeatureTypesBuilder(ICraftsmanUtilities utilities)
    {
        _utilities = utilities;
    }

    public void CreateDynamicFeatureTypes(string spaDirectory, string entityName, string entityPlural, List<BffEntityProperty> props)
    {
        var routesIndexClassPath = ClassPathHelper.BffSpaFeatureClassPath(spaDirectory,
            entityPlural,
            BffFeatureCategory.Types,
            "index.ts");
        var routesIndexFileText = GetAuthFeatureRoutesIndexText(entityName, props);
        _utilities.CreateFile(routesIndexClassPath, routesIndexFileText);
    }

    public static string GetAuthFeatureRoutesIndexText(string entityName, List<BffEntityProperty> props)
    {
        var readDtoName = FileNames.GetDtoName(entityName, Dto.Read);
        var dtoForCreationName = FileNames.GetDtoName(entityName, Dto.Creation);
        var dtoForUpdateName = FileNames.GetDtoName(entityName, Dto.Update);
        var dtoForManipulationName = FileNames.GetDtoName(entityName, Dto.Manipulation);

        var propList = TypePropsBuilder(props);

        return @$"export interface QueryParams {{
  pageNumber?: number;
  pageSize?: number;
  filters?: string;
  sortOrder?: string;
}}

export interface {readDtoName} {{
  id: string;{propList}
}}

export interface {dtoForManipulationName} {{
  id: string;{propList}
}}

export interface {dtoForCreationName} extends {dtoForManipulationName} {{ }}
export interface {dtoForUpdateName} extends {dtoForManipulationName} {{ }}

// need a string enum list?
// const StatusList = ['Status1', 'Status2', null] as const;
// export type Status = typeof StatusList[number];
// Then use as --> status: Status;
";
    }

    private static string TypePropsBuilder(List<BffEntityProperty> props)
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
