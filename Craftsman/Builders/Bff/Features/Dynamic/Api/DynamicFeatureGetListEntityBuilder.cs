namespace Craftsman.Builders.Bff.Features.Dynamic.Api;

using Domain.Enums;
using Helpers;
using Services;

public class DynamicFeatureGetListEntityBuilder
{
	private readonly ICraftsmanUtilities _utilities;

	public DynamicFeatureGetListEntityBuilder(ICraftsmanUtilities utilities)
	{
		_utilities = utilities;
	}

	public void CreateApiFile(string spaDirectory, string entityName, string entityPlural)
	{
		var routesIndexClassPath = ClassPathHelper.BffSpaFeatureClassPath(spaDirectory,
			entityPlural, 
			BffFeatureCategory.Api , 
			$"{FeatureType.GetList.BffApiName(entityName)}.ts");
		var routesIndexFileText = GetApiText(entityName, entityPlural);
		_utilities.CreateFile(routesIndexClassPath, routesIndexFileText);
	}
	
	public static string GetApiText(string entityName, string entityPlural)
	{
		var readDtoName = FileNames.GetDtoName(entityName, Dto.Read);
		var entityPluralUppercaseFirst = entityPlural.UppercaseFirstLetter();
		var entityPluralLowercase = entityPlural.ToLower();
		var keysImport = FileNames.BffApiKeysFilename(entityName);
		var keyExportName = FileNames.BffApiKeysExport(entityName);
		
		return @$"import {{ api }} from '@/lib/axios';
import {{ useQuery }} from 'react-query';
import {{ {keyExportName} }} from './{keysImport}';
import queryString from 'query-string'
import {{ QueryParams, {readDtoName} }} from '../types';
import {{PagedResponse, Pagination}} from '@/types/api';
import {{AxiosResponse}} from 'axios';

const get{entityPluralUppercaseFirst} = (queryString: string) => {{
	queryString = queryString == '' 
		? queryString 
		: `?${{queryString}}`;

	return api.get(`/api/{entityPluralLowercase}?${{queryString}}`)
		.then((response: AxiosResponse<{readDtoName}[]>) => {{
			return {{
				data: response.data as {readDtoName}[],
				pagination: JSON.parse(response.headers['x-pagination']) as Pagination
			}} as PagedResponse<{readDtoName}>;
	}});
}};

export const use{entityPluralUppercaseFirst} = ({{ pageNumber, pageSize, filters, sortOrder }}: QueryParams) => {{
	let queryParams = queryString.stringify({{ pageNumber, pageSize, filters, sortOrder }});

	return useQuery(
		{keyExportName}.list(queryParams ?? ''),
		() => get{entityPluralUppercaseFirst}(queryParams)
	);
}};
";
	}
}
