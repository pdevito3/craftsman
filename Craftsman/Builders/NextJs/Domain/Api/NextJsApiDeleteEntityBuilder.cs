namespace Craftsman.Builders.NextJs.Domain.Api;

using Craftsman.Domain.Enums;
using Craftsman.Helpers;
using Craftsman.Services;

public class NextJsApiDeleteEntityBuilder
{
    private readonly ICraftsmanUtilities _utilities;

    public NextJsApiDeleteEntityBuilder(ICraftsmanUtilities utilities)
    {
        _utilities = utilities;
    }

    public void CreateApiFile(string spaDirectory, string entityName, string entityPlural, string clientName)
    {
        var classPath = ClassPathHelper.NextJsSpaFeatureClassPath(spaDirectory,
            entityPlural,
            NextJsDomainCategory.Api,
            $"{FeatureType.DeleteRecord.NextJsApiName(entityName, entityPlural)}.tsx");
        var fileText = GetApiText(entityName, entityPlural, clientName);
        _utilities.CreateFile(classPath, fileText);
    }

    public static string GetApiText(string entityName, string entityPlural, string clientName)
    {
        var entityPluralLowercase = entityPlural.ToLower();
        var entityUpperFirst = entityName.UppercaseFirstLetter();
        var entityPluralLowercaseFirst = entityPlural.LowercaseFirstLetter();
        var keysImport = FileNames.NextJsApiKeysFilename(entityName);
        var keyExportName = FileNames.NextJsApiKeysExport(entityName);

        return @$"import {{ clients }} from ""@/lib/axios"";
import {{ AxiosError }} from ""axios"";
import {{ useMutation, UseMutationOptions, useQueryClient }} from ""react-query"";
import {{ {keyExportName} }} from ""@/domain/{entityPluralLowercaseFirst}"";

async function delete{entityUpperFirst}(id: string) {{
  const axios = await clients.{clientName}();
  return axios.delete(`/{entityPluralLowercase}/${{id}}`).then(() => {{}});
}}

export function useDelete{entityUpperFirst}(
  options?: UseMutationOptions<void, AxiosError, string>
) {{
  const queryClient = useQueryClient();

  return useMutation((id: string) => delete{entityUpperFirst}(id), {{
    onSuccess: () => {{
      queryClient.invalidateQueries({keyExportName}.lists());
      queryClient.invalidateQueries({keyExportName}.details());
    }},
    ...options,
  }});
}}
";
    }
}
