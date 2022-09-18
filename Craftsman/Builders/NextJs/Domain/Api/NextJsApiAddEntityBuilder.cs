namespace Craftsman.Builders.NextJs.Domain.Api;

using Craftsman.Domain.Enums;
using Craftsman.Helpers;
using Craftsman.Services;

public class NextJsApiAddEntityBuilder
{
    private readonly ICraftsmanUtilities _utilities;

    public NextJsApiAddEntityBuilder(ICraftsmanUtilities utilities)
    {
        _utilities = utilities;
    }

    public void CreateApiFile(string spaDirectory, string entityName, string entityPlural, string clientName)
    {
        var routesIndexClassPath = ClassPathHelper.NextJsSpaFeatureClassPath(spaDirectory,
            entityPlural,
            NextJsDomainCategory.Api,
            $"{FeatureType.AddRecord.NextJsApiName(entityName)}.tsx");
        var routesIndexFileText = GetApiText(entityName, entityPlural, clientName);
        _utilities.CreateFile(routesIndexClassPath, routesIndexFileText);
    }

    public static string GetApiText(string entityName, string entityPlural, string clientName)
    {
        var dtoForCreationName = FileNames.GetDtoName(entityName, Dto.Creation);
        var readDtoName = FileNames.GetDtoName(entityName, Dto.Read);
        var entityPluralLowercase = entityPlural.ToLower();
        var entityPluralLowercaseFirst = entityPlural.LowercaseFirstLetter();
        var entityUpperFirst = entityName.UppercaseFirstLetter();
        var keysImport = FileNames.NextJsApiKeysFilename(entityName);
        var keyExportName = FileNames.NextJsApiKeysExport(entityName);

        return @$"import {{ clients }} from ""@/lib/axios"";
import {{ AxiosError }} from ""axios"";
import {{ useMutation, UseMutationOptions, useQueryClient }} from ""react-query"";
import {{ {readDtoName}, {dtoForCreationName}, {keyExportName} }} from ""@/domain/{entityPluralLowercaseFirst}"";

const add{entityUpperFirst} = async (data: {dtoForCreationName}) => {{
  const axios = await clients.{clientName}();

  return axios
    .post(""/{entityPluralLowercase}"", data)
    .then((response) => response.data as {readDtoName});
}};

export function useAdd{entityUpperFirst}(
  options?: UseMutationOptions<{readDtoName}, AxiosError, {dtoForCreationName}>
) {{
  const queryClient = useQueryClient();

  return useMutation(
    (new{entityUpperFirst}: {dtoForCreationName}) => add{entityUpperFirst}(new{entityUpperFirst}),
    {{
      onSuccess: () => {{
        queryClient.invalidateQueries({keyExportName}.lists());
      }},
      ...options,
    }}
  );
}}
";
    }
}
