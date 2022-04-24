namespace Craftsman.Builders.Bff.Features.Dynamic.Api;

using Domain.Enums;
using Helpers;
using Services;

public class DynamicFeatureDeleteEntityBuilder
{
    private readonly ICraftsmanUtilities _utilities;

    public DynamicFeatureDeleteEntityBuilder(ICraftsmanUtilities utilities)
    {
        _utilities = utilities;
    }

    public void CreateApiFile(string spaDirectory, string entityName, string entityPlural)
    {
        var classPath = ClassPathHelper.BffSpaFeatureClassPath(spaDirectory,
            entityPlural,
            BffFeatureCategory.Api,
            $"{FeatureType.DeleteRecord.BffApiName(entityName)}.ts");
        var fileText = GetApiText(entityName, entityPlural);
        _utilities.CreateFile(classPath, fileText);
    }

    public static string GetApiText(string entityName, string entityPlural)
    {
        var entityPluralLowercase = entityPlural.ToLower();
        var entityUpperFirst = entityName.UppercaseFirstLetter();
        var keysImport = FileNames.BffApiKeysFilename(entityName);
        var keyExportName = FileNames.BffApiKeysExport(entityName);

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
