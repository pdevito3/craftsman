namespace Craftsman.Builders.Bff.Features.Dynamic;

using Domain.Enums;
using Helpers;
using Services;

public class DynamicFeatureRoutesBuilder
{
    private readonly ICraftsmanUtilities _utilities;

    public DynamicFeatureRoutesBuilder(ICraftsmanUtilities utilities)
    {
        _utilities = utilities;
    }

    public void CreateDynamicFeatureRoutes(string spaDirectory, string entityName, string entityPlural)
    {
        var routesIndexClassPath = ClassPathHelper.BffSpaFeatureClassPath(spaDirectory,
            entityPlural,
            BffFeatureCategory.Routes,
            "index.ts");
        var routesIndexFileText = GetAuthFeatureRoutesIndexText(entityName);
        _utilities.CreateFile(routesIndexClassPath, routesIndexFileText);

        var routesLoginFileText = GetEntityListRouteText(entityName, entityPlural);
        var listRouteName = FileNames.BffEntityListRouteComponentName(entityName);
        var routesLoginClassPath = ClassPathHelper.BffSpaFeatureClassPath(spaDirectory,
            entityPlural,
            BffFeatureCategory.Routes,
            $"{listRouteName}.tsx");
        _utilities.CreateFile(routesLoginClassPath, routesLoginFileText);
    }

    public static string GetAuthFeatureRoutesIndexText(string entityName)
    {
        var listRouteName = FileNames.BffEntityListRouteComponentName(entityName);
        return @$"export * from './{listRouteName}';";
    }

    public static string GetEntityListRouteText(string entityName, string entityPlural)
    {
        var entityResponseVar = $"{entityName.LowercaseFirstLetter()}Response";
        var entityDataVar = $"{entityName.LowercaseFirstLetter()}Data";
        var entityPaginationVar = $"{entityName.LowercaseFirstLetter()}Pagination";
        var listRouteName = FileNames.BffEntityListRouteComponentName(entityName);

        return @$"import React from 'react';
import {{ use{entityPlural.UppercaseFirstLetter()} }} from '../api';

function {listRouteName}() {{
    const {{ data: {entityResponseVar}, isLoading }} = use{entityPlural.UppercaseFirstLetter()}({{}});
    const {entityDataVar} = {entityResponseVar}?.data;
    // const {entityPaginationVar} = {entityResponseVar}?.pagination;

    if(isLoading) 
        return <div>Loading...</div>

    return (
        <>
            {{
                {entityDataVar} && {entityDataVar}.length > 0 ? (
                    {entityDataVar}?.map(({entityName.LowercaseFirstLetter()}) => {{
                        return <div key={{{entityName.LowercaseFirstLetter()}.id}}>{{{entityName.LowercaseFirstLetter()}.id}}</div>;
                    }})
                ) : (
                    <div>No {entityPlural} Found</div>
                )}}
        </>
    )
}}

export {{ {listRouteName} }};
";
    }
}
