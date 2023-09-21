namespace Craftsman.Builders.NextJs.Domain.Pages;

using Craftsman.Domain.Enums;
using Craftsman.Helpers;
using Craftsman.Services;

public class NextJsEditEntityPageBuilder
{
    private readonly ICraftsmanUtilities _utilities;

    public NextJsEditEntityPageBuilder(ICraftsmanUtilities utilities)
    {
        _utilities = utilities;
    }

    public void CreateFile(string nextSrc, string entityName, string entityPlural)
    {
        var routesIndexClassPath = ClassPathHelper.NextJsPagesClassPath(nextSrc,
            entityPlural,
            $"[{entityName.LowercaseFirstLetter()}Id].tsx");
        var routesIndexFileText = GetFileText(entityName, entityPlural);
        _utilities.CreateFile(routesIndexClassPath, routesIndexFileText);
    }

    public static string GetFileText(string entityName, string entityPlural)
    {
        var entityPluralLowercase = entityPlural.ToLower();
        var entityPluralLowercaseFirst = entityPlural.LowercaseFirstLetter();
        var entityUpperFirst = entityName.UppercaseFirstLetter();
        var entityLowerFirst = entityName.LowercaseFirstLetter();
        var updatePermission = FeatureType.UpdateRecord.DefaultPermission(entityPlural, null);

        return @$"import {{ PrivateLayout }} from ""@/components"";
import {{ Button }} from ""@/components/forms"";
import {{ Forbidden }} from ""@/domain/auth"";
import {{ useHasPermission }} from ""@/domain/permissions"";
import Head from ""next/head"";
import {{ {FileNames.NextJsEntityFeatureFormName(entityName)}, useGet{entityUpperFirst} }} from ""@/domain/{entityPluralLowercaseFirst}"";
import {{ useRouter }} from ""next/router"";

export default function Edit{entityName}() {{
  const router = useRouter();
  const {{ {entityLowerFirst}Id }} = router.query;
  const {{ data }} = useGet{entityUpperFirst}({entityLowerFirst}Id?.toString());
  const {updatePermission.LowercaseFirstLetter()} = useHasPermission(""{updatePermission}"");

  return (
    <>
      <Head>
        <title>Edit {entityUpperFirst}</title>
      </Head>
      <PrivateLayout>
        {{{updatePermission.LowercaseFirstLetter()}.hasPermission ? (
          <div className=""space-y-6"">
            <div className=""pt-4"">
              <Button buttonStyle=""secondary"" href={{""/{entityPluralLowercase}""}}>
                Back
              </Button>
            </div>
            <div className="""">
              <h1 className=""h1"">Edit {entityName.UppercaseFirstLetter()}</h1>
              <div className=""max-w-3xl py-6 space-y-5"">
                <{FileNames.NextJsEntityFeatureFormName(entityName)} {entityLowerFirst}Id={{{entityLowerFirst}Id?.toString()}} {entityLowerFirst}Data={{data}} />
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
