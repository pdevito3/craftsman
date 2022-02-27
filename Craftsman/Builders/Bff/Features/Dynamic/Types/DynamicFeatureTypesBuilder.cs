namespace Craftsman.Builders.Bff.Features.Dynamic.Types;

using System.IO.Abstractions;
using Enums;
using Helpers;
using Models;

public class DynamicFeatureTypesBuilder
{
	public static void CreateDynamicFeatureTypes(string spaDirectory, string entityName, List<BffEntityProperty> props, IFileSystem fileSystem)
	{
		var routesIndexClassPath = ClassPathHelper.BffSpaFeatureClassPath(spaDirectory, 
			entityName, 
			BffFeatureCategory.Types , 
			"index.ts");
		var routesIndexFileText = GetAuthFeatureRoutesIndexText(entityName, props);
		Utilities.CreateFile(routesIndexClassPath, routesIndexFileText, fileSystem);
	}
	
	public static string GetAuthFeatureRoutesIndexText(string entityName, List<BffEntityProperty> props)
	{
		var readDtoName = Utilities.GetDtoName(entityName, Dto.Read);
		var dtoForCreationName = Utilities.GetDtoName(entityName, Dto.Creation);
		var dtoForUpdateName = Utilities.GetDtoName(entityName, Dto.Update);
		var dtoForManipulationName = Utilities.GetDtoName(entityName, Dto.Manipulation);

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
// export const Status = ['Status1', 'Status2', null] as const;
// status: typeof Status[number];
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
			propString += $@"{Environment.NewLine}  {bffEntityProperty.Name}{questionMark}: {bffEntityProperty.RawType};";
		}

		return propString;
	}
	
	
}
