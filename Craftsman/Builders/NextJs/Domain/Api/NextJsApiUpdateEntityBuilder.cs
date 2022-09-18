namespace Craftsman.Builders.NextJs.Domain.Api;

using Craftsman.Domain.Enums;
using Craftsman.Helpers;
using Craftsman.Services;

public class NextJsApiUpdateEntityBuilder
{
    private readonly ICraftsmanUtilities _utilities;

    public NextJsApiUpdateEntityBuilder(ICraftsmanUtilities utilities)
    {
        _utilities = utilities;
    }

    public void CreateApiFile(string spaDirectory, string entityName, string entityPlural, string clientName)
    {
        var routesIndexClassPath = ClassPathHelper.NextJsSpaFeatureClassPath(spaDirectory,
            entityPlural,
            NextJsDomainCategory.Api,
            $"{FeatureType.UpdateRecord.NextJsApiName(entityName)}.tsx");
        var routesIndexFileText = GetApiText(entityName, entityPlural, clientName);
        _utilities.CreateFile(routesIndexClassPath, routesIndexFileText);
    }

    public static string GetApiText(string entityName, string entityPlural, string clientName)
    {
        var dtoForUpdateName = FileNames.GetDtoName(entityName, Dto.Update);
        var entityPluralLowercase = entityPlural.ToLower();
        var entityUpperFirst = entityName.UppercaseFirstLetter();
        var entityPluralLowercaseFirst = entityPlural.LowercaseFirstLetter();
        var entityLowerFirst = entityName.LowercaseFirstLetter();
        var keysImport = FileNames.NextJsApiKeysFilename(entityName);
        var keyExportName = FileNames.NextJsApiKeysExport(entityName);

        return @$"import {{ clients }} from ""@/lib/axios"";
import {{ AxiosError }} from ""axios"";
import {{ useMutation, UseMutationOptions, useQueryClient }} from ""react-query"";
import {{ {dtoForUpdateName}, {keyExportName} }} from ""@/domain/{entityPluralLowercaseFirst}"";

const update{entityUpperFirst} = async (id: string, data: {dtoForUpdateName}) => {{
  const axios = await clients.{clientName}();
  return axios.put(`/{entityPluralLowercase}/${{id}}`, data).then((response) => response.data);
}};

export interface UpdateProps {{
  id: string;
  data: {dtoForUpdateName};
}}

export function useUpdate{entityUpperFirst}(
  options?: UseMutationOptions<void, AxiosError, UpdateProps>
) {{
  const queryClient = useQueryClient();

  return useMutation(
    ({{ id, data: updated{entityUpperFirst} }}: UpdateProps) =>
      update{entityUpperFirst}(id, updated{entityUpperFirst}),
    {{
      onSuccess: () => {{
        queryClient.invalidateQueries({keyExportName}.lists());
        queryClient.invalidateQueries({keyExportName}.details());
      }},
      ...options,
    }}
  );
}}
";
    }
}
