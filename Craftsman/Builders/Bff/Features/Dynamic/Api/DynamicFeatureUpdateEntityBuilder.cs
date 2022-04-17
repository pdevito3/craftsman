namespace Craftsman.Builders.Bff.Features.Dynamic.Api;

using System.IO.Abstractions;
using Enums;
using Helpers;
using Models;

public class DynamicFeatureUpdateEntityBuilder
{
	public static void CreateApiFile(string spaDirectory, string entityName, string entityPlural, IFileSystem fileSystem)
	{
		var routesIndexClassPath = ClassPathHelper.BffSpaFeatureClassPath(spaDirectory, 
			entityPlural, 
			BffFeatureCategory.Api , 
			$"{FeatureType.UpdateRecord.BffApiName(entityName)}.ts");
		var routesIndexFileText = GetApiText(entityName, entityPlural);
		Utilities.CreateFile(routesIndexClassPath, routesIndexFileText, fileSystem);
	}
	
	public static string GetApiText(string entityName, string entityPlural)
	{
		var dtoForUpdateName = Utilities.GetDtoName(entityName, Dto.Update);
		var entityPluralLowercase = entityPlural.ToLower();
		var entityUpperFirst = entityName.UppercaseFirstLetter();
		var keysImport = Utilities.BffApiKeysFilename(entityName);
		var keyExportName = Utilities.BffApiKeysExport(entityName);
		
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
