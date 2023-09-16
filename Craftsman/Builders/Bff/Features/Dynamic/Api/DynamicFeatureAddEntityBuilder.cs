namespace Craftsman.Builders.Bff.Features.Dynamic.Api;

using Domain.Enums;
using Helpers;
using Services;

public class DynamicFeatureAddEntityBuilder
{
    private readonly ICraftsmanUtilities _utilities;

    public DynamicFeatureAddEntityBuilder(ICraftsmanUtilities utilities)
    {
        _utilities = utilities;
    }

    public void CreateApiFile(string spaDirectory, string entityName, string entityPlural)
    {
        var routesIndexClassPath = ClassPathHelper.BffSpaFeatureClassPath(spaDirectory,
            entityPlural,
            BffFeatureCategory.Api,
            $"{FeatureType.AddRecord.BffApiName(entityName, entityPlural)}.ts");
        var routesIndexFileText = GetApiText(entityName, entityPlural);
        _utilities.CreateFile(routesIndexClassPath, routesIndexFileText);
    }

    public static string GetApiText(string entityName, string entityPlural)
    {
        var dtoForCreationName = FileNames.GetDtoName(entityName, Dto.Creation);
        var readDtoName = FileNames.GetDtoName(entityName, Dto.Read);
        var entityPluralLowercase = entityPlural.ToLower();
        var entityUpperFirst = entityName.UppercaseFirstLetter();
        var keysImport = FileNames.BffApiKeysFilename(entityName);
        var keyExportName = FileNames.BffApiKeysExport(entityName);

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
