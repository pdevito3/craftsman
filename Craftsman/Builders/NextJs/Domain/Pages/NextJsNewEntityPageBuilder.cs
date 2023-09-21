namespace Craftsman.Builders.NextJs.Domain.Pages;

using Craftsman.Domain.Enums;
using Craftsman.Helpers;
using Craftsman.Services;

public class NextJsNewEntityPageBuilder
{
    private readonly ICraftsmanUtilities _utilities;

    public NextJsNewEntityPageBuilder(ICraftsmanUtilities utilities)
    {
        _utilities = utilities;
    }

    public void CreateFile(string nextSrc, string entityName, string entityPlural)
    {
        var routesIndexClassPath = ClassPathHelper.NextJsPagesClassPath(nextSrc,
            entityPlural,
            $"new.tsx");
        var routesIndexFileText = GetFileText(entityName, entityPlural);
        _utilities.CreateFile(routesIndexClassPath, routesIndexFileText);
    }

    public static string GetFileText(string entityName, string entityPlural)
    {
        var entityPluralLowercase = entityPlural.ToLower();
        var entityNameUpperFirst = entityName.UppercaseFirstLetter();
        var entityPluralLowercaseFirst = entityPlural.LowercaseFirstLetter();
        var formName = FileNames.NextJsEntityFeatureFormName(entityName);
        var entityStartsWithVowel = "aeiouAEIOU".IndexOf(entityName) >= 0;
        var aOrAn = entityStartsWithVowel ? "an" : "a";
        var addPermission = FeatureType.AddRecord.DefaultPermission(entityPlural, null);

        return @$"import {{ PrivateLayout }} from ""@/components"";
import {{ Button }} from ""@/components/forms"";
import {{ Forbidden }} from ""@/domain/auth"";
import {{ useHasPermission }} from ""@/domain/permissions"";
import {{ {formName} }} from ""@/domain/{entityPluralLowercaseFirst}"";
import Head from ""next/head"";

export default function New{entityName}() {{
  const {addPermission.LowercaseFirstLetter()} = useHasPermission(""{addPermission}"");

  return (
    <>
      <Head>
        <title>Add {entityNameUpperFirst}</title>
      </Head>
      <PrivateLayout>
        {{{addPermission.LowercaseFirstLetter()}.hasPermission ? (
          <div className=""space-y-6"">
            <div className=""pt-4"">
              <Button buttonStyle=""secondary"" href={{""/{entityPluralLowercase}""}}>
                Back
              </Button>
            </div>
            <div className="""">
              <h1 className=""h1"">Add {aOrAn} {entityNameUpperFirst}</h1>
              <div className=""max-w-3xl py-6 space-y-5"">
                <{formName} />
              </div>
            </div>
          </div>
        ) : (
          <Forbidden/>
        )}}
      </PrivateLayout>
    </>
  );
}}";
    }
}
