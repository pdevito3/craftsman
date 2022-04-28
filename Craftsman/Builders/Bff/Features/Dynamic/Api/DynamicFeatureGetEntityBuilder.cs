namespace Craftsman.Builders.Bff.Features.Dynamic.Api;

using System.IO.Abstractions;
using Enums;
using Helpers;
using Models;

public class DynamicFeatureGetEntityBuilder
{
	public static void CreateApiFile(string spaDirectory, string entityName, string entityPlural, IFileSystem fileSystem)
	{
		var classPath = ClassPathHelper.BffSpaFeatureClassPath(spaDirectory, 
			entityPlural, 
			BffFeatureCategory.Api , 
			$"{FeatureType.GetRecord.BffApiName(entityName)}.ts");
		var fileText = GetApiText(entityName, entityPlural);
		Utilities.CreateFile(classPath, fileText, fileSystem);
	}
	
	public static string GetApiText(string entityName, string entityPlural)
	{
		var entityPluralLowercase = entityPlural.ToLower();
		var readDtoName = Utilities.GetDtoName(entityName, Dto.Read);
		var entityUpperFirst = entityName.UppercaseFirstLetter();
		var keysImport = Utilities.BffApiKeysFilename(entityName);
		var keyExportName = Utilities.BffApiKeysExport(entityName);
		
		return @$"import {{ api }} from '@/lib/axios';
import {{ AxiosResponse }} from 'axios';
import {{ useQuery }} from 'react-query';
import {{ {readDtoName} }} from '../types';
import {{ {keyExportName} }} from './{keysImport}';

export const get{entityUpperFirst} = (id: string) => {{
	return api
		.get(`/api/{entityPluralLowercase}/${{id}}`)
		.then((response: AxiosResponse<{readDtoName}>) => response.data);
}};

export const useGet{entityUpperFirst} = (id: string) => {{
	return useQuery({keyExportName}.detail(id), () => get{entityUpperFirst}(id));
}};
";
	}
}
