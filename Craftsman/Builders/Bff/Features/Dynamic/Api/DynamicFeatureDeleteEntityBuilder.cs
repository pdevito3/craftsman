namespace Craftsman.Builders.Bff.Features.Dynamic.Api;

using System.IO.Abstractions;
using Enums;
using Helpers;
using Models;

public class DynamicFeatureDeleteEntityBuilder
{
	public static void CreateApiFile(string spaDirectory, string entityName, string entityPlural, IFileSystem fileSystem)
	{
		var classPath = ClassPathHelper.BffSpaFeatureClassPath(spaDirectory, 
			entityPlural, 
			BffFeatureCategory.Api , 
			$"{FeatureType.DeleteRecord.BffApiName(entityName)}.ts");
		var fileText = GetApiText(entityName, entityPlural);
		Utilities.CreateFile(classPath, fileText, fileSystem);
	}
	
	public static string GetApiText(string entityName, string entityPlural)
	{
		var entityPluralLowercase = entityPlural.ToLower();
		var entityUpperFirst = entityName.UppercaseFirstLetter();
		var keysImport = Utilities.BffApiKeysFilename(entityName);
		var keyExportName = Utilities.BffApiKeysExport(entityName);
		
		return @$"import {{ api }} from '@/lib/axios';
import {{ AxiosError }} from 'axios';
import {{ UseMutationOptions, useQueryClient, useMutation }} from 'react-query';
import {{ {keyExportName} }} from './{keysImport}';

async function delete{entityUpperFirst}(id: string) {{
	return api
		.delete(`/api/{entityPluralLowercase}/${{id}}`)
		.then(() => {{ }});
}}

export function useDelete{entityUpperFirst}(options?: UseMutationOptions<void, AxiosError, string>) {{
	const queryClient = useQueryClient()

	return useMutation(
		(id: string) => delete{entityUpperFirst}(id),
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
