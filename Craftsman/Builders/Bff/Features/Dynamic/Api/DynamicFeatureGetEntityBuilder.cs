namespace Craftsman.Builders.Bff.Features.Dynamic.Api;

using Domain.Enums;
using Helpers;
using Services;

public class DynamicFeatureGetEntityBuilder
{
	private readonly ICraftsmanUtilities _utilities;

	public DynamicFeatureGetEntityBuilder(ICraftsmanUtilities utilities)
	{
		_utilities = utilities;
	}

	public void CreateApiFile(string spaDirectory, string entityName, string entityPlural)
	{
		var classPath = ClassPathHelper.BffSpaFeatureClassPath(spaDirectory, 
			entityPlural, 
			BffFeatureCategory.Api , 
			$"{FeatureType.GetRecord.BffApiName(entityName)}.ts");
		var fileText = GetApiText(entityName, entityPlural);
		_utilities.CreateFile(classPath, fileText);
	}
	
	public static string GetApiText(string entityName, string entityPlural)
	{
		var entityPluralLowercase = entityPlural.ToLower();
		var readDtoName = FileNames.GetDtoName(entityName, Dto.Read);
		var entityUpperFirst = entityName.UppercaseFirstLetter();
		var keysImport = FileNames.BffApiKeysFilename(entityName);
		var keyExportName = FileNames.BffApiKeysExport(entityName);
		
		return @$"import {{ api }} from '@/lib/axios';
import {{ AxiosResponse }} from 'axios';
import {{ UseMutationOptions, useQueryClient, useMutation }} from 'react-query';
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
