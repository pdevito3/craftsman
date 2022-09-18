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
        var entityNameLowerFirst = entityName.LowercaseFirstLetter();
        var entityNameUpperFirst = entityName.UppercaseFirstLetter();
        var formName = FileNames.NextJsEntityFeatureFormName(entityName);

        return @$"import {{ PrivateLayout }} from ""@/components"";
import {{ Button }} from ""@/components/forms"";
import {{ {formName}, useGet{entityNameUpperFirst} }} from ""@/domain/{entityPluralLowercase}"";
import {{ useRouter }} from ""next/router"";

export default function Edit{entityNameUpperFirst}() {{
  const router = useRouter();
  const {{ {entityNameLowerFirst}Id }} = router.query;
  const {{ data }} = useGet{entityNameUpperFirst}({entityNameLowerFirst}Id?.toString());

  return (
    <PrivateLayout>
      <div className=""space-y-6"">
        <Button href={{""/{entityPluralLowercase}""}}>Back</Button>
        <div className="""">
          <h1 className=""h1"">Edit {entityNameUpperFirst}</h1>
          <div className=""py-6"">
            <{formName} {entityNameLowerFirst}Id={{{entityNameLowerFirst}Id?.toString()}} {entityNameLowerFirst}Data={{data}} />
          </div>
        </div>
      </div>
    </PrivateLayout>
  );
}}

";
    }
}
