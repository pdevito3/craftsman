namespace Craftsman.Builders.NextJs.Domain.Pages;

using Craftsman.Domain;
using Craftsman.Domain.Enums;
using Craftsman.Helpers;
using Craftsman.Services;

public class NextJsEntityIndexPageBuilder
{
    private readonly ICraftsmanUtilities _utilities;

    public NextJsEntityIndexPageBuilder(ICraftsmanUtilities utilities)
    {
        _utilities = utilities;
    }

    public void CreateFile(string nextSrc, string entityName, string entityPlural, List<NextJsEntityProperty> properties)
    {
        var routesIndexClassPath = ClassPathHelper.NextJsPagesClassPath(nextSrc,
            entityPlural,
            $"index.tsx");
        var routesIndexFileText = GetFileText(entityName, entityPlural, properties);
        _utilities.CreateFile(routesIndexClassPath, routesIndexFileText);
    }

    public static string GetFileText(string entityName, string entityPlural, List<NextJsEntityProperty> properties)
    {
        var entityPluralLowercase = entityPlural.ToLower();
        var entityUpperFirst = entityName.UppercaseFirstLetter();
        var entityPluralLowercaseFirst = entityPlural.LowercaseFirstLetter();

        return @$"import {{ PrivateLayout, SearchInput }} from ""@/components"";
import {{
  Button,
  PaginatedTableProvider,
  useGlobalFilter,
}} from ""@/components/forms"";
import {{ {FileNames.NextJsEntityFeatureListTableName(entityName)} }} from ""@/domain/{entityPluralLowercaseFirst}"";
import ""@tanstack/react-table"";
import {{ IconCirclePlus }} from ""@tabler/icons"";

{entityUpperFirst}List.isPublic = false;
export default function {entityUpperFirst}List() {{
  const {{
    globalFilter,
    queryFilter,
    calculateAndSetQueryFilter,
  }} = useGlobalFilter((value) => `({string.Join("|", properties.Select(x => x.Name.LowercaseFirstLetter()))})@=*${{value}}`);

  return (
    <>
      <Head>
        <title>{entityPlural}</title>
      </Head>

      <PrivateLayout>
        <div className=""space-y-6 max-w-9xl"">
          <div className="""">
            <h1 className=""h1"">{entityPlural.UppercaseFirstLetter()}</h1>
            <div className=""py-4"">
              <PaginatedTableProvider>
                <div className=""flex items-center justify-between"">
                  <div className=""mt-1"">
                    <SearchInput
                      value={{globalFilter ?? """"}}
                      onChange={{(value) =>
                        calculateAndSetQueryFilter(String(value))
                      }}
                      placeholder=""Search all columns...""
                    />
                  </div>

                  <Button
                    buttonStyle=""primary""
                    icon={{<IconCirclePlus className=""w-5 h-5"" />}}
                    href=""/{entityPluralLowercase}/new""
                  >
                    Add {entityName}
                  </Button>
                </div>

                <div className=""pt-2"">
                  <{FileNames.NextJsEntityFeatureListTableName(entityName)} queryFilter={{queryFilter}} />
                </div>
              </PaginatedTableProvider>
            </div>
          </div>
        </div>
      </PrivateLayout>
    </>
  );
}}
";
    }
}
