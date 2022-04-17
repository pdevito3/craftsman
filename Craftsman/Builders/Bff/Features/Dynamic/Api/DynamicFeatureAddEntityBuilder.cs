namespace Craftsman.Builders.Bff.Features.Dynamic.Api;

using System.IO.Abstractions;
using Enums;
using Helpers;
using Models;

public class DynamicFeatureAddEntityBuilder
{
	public static void CreateApiFile(string spaDirectory, string entityName, string entityPlural, IFileSystem fileSystem)
	{
		var routesIndexClassPath = ClassPathHelper.BffSpaFeatureClassPath(spaDirectory, 
			entityPlural, 
			BffFeatureCategory.Api , 
			$"{FeatureType.AddRecord.BffApiName(entityName)}.ts");
		var routesIndexFileText = GetApiText(entityName, entityPlural);
		Utilities.CreateFile(routesIndexClassPath, routesIndexFileText, fileSystem);
	}
	
	public static string GetApiText(string entityName, string entityPlural)
	{
		var dtoForCreationName = Utilities.GetDtoName(entityName, Dto.Creation);
		var readDtoName = Utilities.GetDtoName(entityName, Dto.Read);
		var entityPluralLowercase = entityPlural.ToLower();
		var entityUpperFirst = entityName.UppercaseFirstLetter();
		var keysImport = Utilities.BffApiKeysFilename(entityName);
		var keyExportName = Utilities.BffApiKeysExport(entityName);
		
		return @$"import {{ api }} from '@/lib/axios';
import {{ AxiosError }} from 'axios';
import {{ UseMutationOptions, useQueryClient, useMutation }} from 'react-query';
import {{ {keyExportName} }} from './{keysImport}';
import {{ {readDtoName}, {dtoForCreationName} }} from '../types';

const add{entityUpperFirst} = (data: {dtoForCreationName}) => {{
	return api
		.post('/api/{entityPluralLowercase}', data)
		.then((response) => response.data as {readDtoName});
}};

export function useAdd{entityUpperFirst}(options?: UseMutationOptions<{readDtoName}, AxiosError, {dtoForCreationName}>) {{
	const queryClient = useQueryClient()

	return useMutation(
		(new{entityUpperFirst}: {dtoForCreationName}) => add{entityUpperFirst}(new{entityUpperFirst}),
		{{
			onSuccess: () => {{
				queryClient.invalidateQueries({keyExportName}.lists())
			}},
			...options
		}});
}}
";
	}
}
