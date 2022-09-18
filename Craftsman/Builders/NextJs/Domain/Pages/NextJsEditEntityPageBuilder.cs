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
            $"new.tsx");
        var routesIndexFileText = GetFileText(entityName, entityPlural);
        _utilities.CreateFile(routesIndexClassPath, routesIndexFileText);
    }

    public static string GetFileText(string entityName, string entityPlural)
    {
        var entityPluralLowercase = entityPlural.ToLower();
        var entityUpperFirst = entityName.UppercaseFirstLetter();
        var entityStartsWithVowel = "aeiouAEIOU".IndexOf(entityName) >= 0;
        var aOrAn = entityStartsWithVowel ? "an" : "a";

        return @$"import {{ PrivateLayout }} from ""@/components"";
import {{ Button }} from ""@/components/forms"";
import {{ {FileNames.NextJsEntityFeatureFormName(entityName)} }} from ""@/domain/recipes"";

export default function New{entityUpperFirst}() {{
  return (
    <PrivateLayout>
      <div className=""space-y-6"">
        <Button
          href={{""/{entityPluralLowercase}""}}
        >
          Back
        </Button>
        <div className="""">
          <h1 className=""h1"">Add {aOrAn} {entityUpperFirst}</h1>
          <div className=""py-6"">
            <{FileNames.NextJsEntityFeatureFormName(entityName)} />
          </div>
        </div>
      </div>
    </PrivateLayout>
  );
}}
";
    }
}
