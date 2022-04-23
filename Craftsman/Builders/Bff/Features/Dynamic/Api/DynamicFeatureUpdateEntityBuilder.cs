namespace Craftsman.Builders.Bff.Features.Dynamic.Api;

using Domain.Enums;
using Helpers;
using Services;

public class DynamicFeatureUpdateEntityBuilder
{
	private readonly ICraftsmanUtilities _utilities;

	public DynamicFeatureUpdateEntityBuilder(ICraftsmanUtilities utilities)
	{
		_utilities = utilities;
	}

	public void CreateApiFile(string spaDirectory, string entityName, string entityPlural)
	{
		var routesIndexClassPath = ClassPathHelper.BffSpaFeatureClassPath(spaDirectory, 
			entityPlural, 
			BffFeatureCategory.Api , 
			$"{FeatureType.UpdateRecord.BffApiName(entityName)}.ts");
		var routesIndexFileText = GetApiText(entityName, entityPlural);
		_utilities.CreateFile(routesIndexClassPath, routesIndexFileText);
	}
	
	public static string GetApiText(string entityName, string entityPlural)
	{
		var dtoForUpdateName = FileNames.GetDtoName(entityName, Dto.Update);
		var entityPluralLowercase = entityPlural.ToLower();
		var entityUpperFirst = entityName.UppercaseFirstLetter();
		var keysImport = FileNames.BffApiKeysFilename(entityName);
		var keyExportName = FileNames.BffApiKeysExport(entityName);
		
		return @$"import {{ api }} from '@/lib/axios';
import {{ AxiosError }} from 'axios';
import {{ UseMutationOptions, useQueryClient, useMutation }} from 'react-query';
import {{ {keyExportName} }} from './{keysImport}';
import {{ {dtoForUpdateName} }} from '../types';

export const update{entityUpperFirst} = (id: string, data: {dtoForUpdateName}) => {{
	return api
		.put(`/api/{entityPluralLowercase}/${{id}}`, data)
		.then(() => {{ }});
}};

export function useUpdate{entityUpperFirst}(id: string, options?: UseMutationOptions<void, AxiosError, {dtoForUpdateName}>) {{
	const queryClient = useQueryClient()

	return useMutation(
		(updated{entityUpperFirst}: {dtoForUpdateName}) => update{entityUpperFirst}(id, updated{entityUpperFirst}),
		{{
			onSuccess: () => {{
				queryClient.invalidateQueries({keyExportName}.lists())
				queryClient.invalidateQueries({keyExportName}.details())
			}},
			...options
		}});
}}
";
	}
}
