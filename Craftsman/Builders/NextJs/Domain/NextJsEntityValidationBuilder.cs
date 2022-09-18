namespace Craftsman.Builders.NextJs.Domain;

using Craftsman.Domain;
using Craftsman.Domain.Enums;
using Craftsman.Helpers;
using Craftsman.Services;

public class NextJsEntityValidationBuilder
{
    private readonly ICraftsmanUtilities _utilities;

    public NextJsEntityValidationBuilder(ICraftsmanUtilities utilities)
    {
        _utilities = utilities;
    }

    public void CreateFile(string nextSrc, string entityName, string entityPlural, List<NextJsEntityProperty> properties)
    {
        var routesIndexClassPath = ClassPathHelper.NextJsSpaFeatureClassPath(nextSrc,
            entityPlural,
            NextJsDomainCategory.Features,
            $"{FileNames.NextJsEntityFeatureFormName(entityName)}.tsx");
        var routesIndexFileText = GetFileText(entityName, properties);
        _utilities.CreateFile(routesIndexClassPath, routesIndexFileText);
    }

    public static string GetFileText(string entityName, List<NextJsEntityProperty> properties)
    {
        var validationSchema = FileNames.NextJsEntityValidationName(entityName);

        return @$"import * as yup from ""yup"";

export const {validationSchema} = yup.object({{{GetValidations(properties)}
}});";
    }

    private static string GetValidations(List<NextJsEntityProperty> properties)
    {
        var validations = "";
        foreach (var property in properties)
        {
            validations += property.TypeEnum.YupValidation(property.Name);
        }

        return validations;
    }

    private static string GetFormControls(List<NextJsEntityProperty> properties, string validationSchema)
    {
        var controls = "";
        foreach (var property in properties)
        {
            controls += property.TypeEnum.FormControl(property.Name, property.Label, validationSchema);
        }

        return controls;
    }

    private static string GetSetValues(List<NextJsEntityProperty> properties, string entityName)
    {
        var setValues = "";
        foreach (var property in properties)
        {
            setValues += property.TypeEnum.FormSetValue(property.Name, entityName);
        }

        return setValues;
    }
}
