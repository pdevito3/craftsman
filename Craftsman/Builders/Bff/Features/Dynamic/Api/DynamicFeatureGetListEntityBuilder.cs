namespace Craftsman.Builders.Bff.Features.Dynamic.Api;

using System.IO.Abstractions;
using Enums;
using Helpers;
using Models;

public class DynamicFeatureGetListEntityBuilder
{
	public static void CreateApiFile(string spaDirectory, string entityName, string entityPlural, IFileSystem fileSystem)
	{
		var routesIndexClassPath = ClassPathHelper.BffSpaFeatureClassPath(spaDirectory,
			entityPlural, 
			BffFeatureCategory.Api , 
			$"{FeatureType.GetList.BffApiName(entityName)}.ts");
		var routesIndexFileText = GetApiText(entityName, entityPlural);
		Utilities.CreateFile(routesIndexClassPath, routesIndexFileText, fileSystem);
	}
	
	public static string GetApiText(string entityName, string entityPlural)
	{
		var readDtoName = Utilities.GetDtoName(entityName, Dto.Read);
		var entityPluralUppercaseFirst = entityPlural.UppercaseFirstLetter();
		var entityPluralLowercase = entityPlural.ToLower();
		var keysImport = Utilities.BffApiKeysFilename(entityName);
		var keyExportName = Utilities.BffApiKeysExport(entityName);
		
		return @$"import {{ api }} from '@/lib/axios';
import {{ useQuery }} from 'react-query';
import {{ {keyExportName} }} from './{keysImport}';
import queryString from 'query-string'
import {{ QueryParams, {readDtoName} }} from '../types';
import {{PagedResponse, Pagination}} from '@/types/api';

const get{entityPluralUppercaseFirst} = (queryString: string) => {{
	return api.get(`/api/{entityPluralLowercase}?${{queryString}}`).then((response) => {{
		return {{
			data: response,
			pagination: JSON.parse(response.headers['x-pagination']) as Pagination
		}} as PagedResponse<{readDtoName}>;
	}});
}};

export const use{entityPluralUppercaseFirst} = ({{ pageNumber, pageSize, filters, sortOrder }}: QueryParams = {{}}) => {{
	const queryParams = queryString.stringify({{ pageNumber, pageSize, filters, sortOrder }});

	return useQuery(
		{keyExportName}.list(queryParams ?? ''),
		() => get{entityPluralUppercaseFirst}(queryParams)
	);
}};
";
	}
}
