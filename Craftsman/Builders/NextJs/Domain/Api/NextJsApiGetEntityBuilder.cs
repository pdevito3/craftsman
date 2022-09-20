namespace Craftsman.Builders.NextJs.Domain.Api;

using Craftsman.Domain.Enums;
using Craftsman.Helpers;
using Craftsman.Services;

public class NextJsApiGetEntityBuilder
{
    private readonly ICraftsmanUtilities _utilities;

    public NextJsApiGetEntityBuilder(ICraftsmanUtilities utilities)
    {
        _utilities = utilities;
    }

    public void CreateApiFile(string spaDirectory, string entityName, string entityPlural, string clientName)
    {
        var classPath = ClassPathHelper.NextJsSpaFeatureClassPath(spaDirectory,
            entityPlural,
            NextJsDomainCategory.Api,
            $"{FeatureType.GetRecord.NextJsApiName(entityName)}.tsx");
        var fileText = GetApiText(entityName, entityPlural, clientName);
        _utilities.CreateFile(classPath, fileText);
    }

    public static string GetApiText(string entityName, string entityPlural, string clientName)
    {
        var entityPluralLowercase = entityPlural.ToLower();
        var readDtoName = FileNames.GetDtoName(entityName, Dto.Read);
        var entityUpperFirst = entityName.UppercaseFirstLetter();
        var entityPluralLowercaseFirst = entityPlural.LowercaseFirstLetter();
        var keysImport = FileNames.NextJsApiKeysFilename(entityName);
        var keyExportName = FileNames.NextJsApiKeysExport(entityName);

        return @$"import {{ clients }} from ""@/lib/axios"";
import {{ AxiosResponse }} from ""axios"";
import {{ useQuery }} from ""react-query"";
import {{ {readDtoName}, {keyExportName} }} from ""@/domain/{entityPluralLowercaseFirst}"";

const get{entityUpperFirst} = async (id: string) => {{
  const axios = await clients.{clientName}();

  return axios
    .get(`/{entityPluralLowercase}/${{id}}`)
    .then((response: AxiosResponse<{readDtoName}>) => response.data);
}};

export const useGet{entityUpperFirst} = (id: string | null | undefined) => {{
  return useQuery({keyExportName}.detail(id!), () => get{entityUpperFirst}(id!), {{
    enabled: id !== null && id !== undefined,
  }});
}};";
    }
}
