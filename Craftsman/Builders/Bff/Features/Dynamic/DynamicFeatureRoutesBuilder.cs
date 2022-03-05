namespace Craftsman.Builders.Bff.Features.Dynamic;

using System.IO.Abstractions;
using Enums;
using Helpers;

public class DynamicFeatureRoutesBuilder
{
	public static void CreateDynamicFeatureRoutes(string spaDirectory, string entityName, string entityPlural, IFileSystem fileSystem)
	{
		var routesIndexClassPath = ClassPathHelper.BffSpaFeatureClassPath(spaDirectory, 
			entityPlural, 
			BffFeatureCategory.Routes ,
			"index.ts");
		var routesIndexFileText = GetAuthFeatureRoutesIndexText(entityName);
		Utilities.CreateFile(routesIndexClassPath, routesIndexFileText, fileSystem);

		var routesLoginFileText = GetEntityListRouteText(entityName, entityPlural);
		var listRouteName = Utilities.BffEntityListRouteComponentName(entityName);
		var routesLoginClassPath = ClassPathHelper.BffSpaFeatureClassPath(spaDirectory, 
			entityPlural, 
			BffFeatureCategory.Routes , 
			$"{listRouteName}.tsx");
		Utilities.CreateFile(routesLoginClassPath, routesLoginFileText, fileSystem);
	}
	
	public static string GetAuthFeatureRoutesIndexText(string entityName)
	{
		var listRouteName = Utilities.BffEntityListRouteComponentName(entityName);
	    return @$"export * from './{listRouteName}';";
	}

public static string GetEntityListRouteText(string entityName, string entityPlural)
{
	var entityResponseVar = $"{entityName.LowercaseFirstLetter()}Response";
	var entityDataVar = $"{entityName.LowercaseFirstLetter()}Data";
	var entityPaginationVar = $"{entityName.LowercaseFirstLetter()}Pagination";
	var listRouteName = Utilities.BffEntityListRouteComponentName(entityName);
	
    return @$"import React from 'react';
import {{ use{entityPlural.UppercaseFirstLetter()} }} from '../api';

function {listRouteName}() {{
	const {{ data: {entityResponseVar} }} = use{entityPlural.UppercaseFirstLetter()}();
	const {entityDataVar} = {entityPlural.LowercaseFirstLetter()}?.data;
	// const {entityPaginationVar} = {entityPlural.LowercaseFirstLetter()}?.pagination;

	return <>
		{{
			{entityDataVar} && {entityDataVar}?.map(({entityName.LowercaseFirstLetter()}) => {{
				return <div key={{{entityName.LowercaseFirstLetter()}.id}}>{{{entityName.LowercaseFirstLetter()}.id}}</div>;
			}})
		}}
	</>;
}}

export {{ {listRouteName} }};
";
	}
}
