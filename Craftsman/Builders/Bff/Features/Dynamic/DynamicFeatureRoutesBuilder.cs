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
	var actualDataVar = $"{entityName.LowercaseFirstLetter()}Data";
	var listRouteName = Utilities.BffEntityListRouteComponentName(entityName);
	
    return @$"import React from 'react';
import {{ use{entityPlural.UppercaseFirstLetter()} }} from '../api';

function {listRouteName}() {{
	const {{ data: {entityPlural.LowercaseFirstLetter()} }} = use{entityPlural.UppercaseFirstLetter()}();
	const {actualDataVar} = {entityPlural.LowercaseFirstLetter()}?.data;

	return <>
		{{
			{actualDataVar} && {actualDataVar}?.map(({entityName.LowercaseFirstLetter()}) => {{
				return <div key={{{entityName.LowercaseFirstLetter()}.id}}>{{{entityName.LowercaseFirstLetter()}.id}}</div>;
			}})
		}}
	</>;
}}

export {{ {listRouteName} }};
";
	}
}
