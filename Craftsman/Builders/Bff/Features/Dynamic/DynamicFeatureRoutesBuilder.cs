namespace Craftsman.Builders.Bff.Features.Dynamic;

using System.IO.Abstractions;
using Enums;
using Helpers;

public class DynamicFeatureRoutesBuilder
{
	public static void CreateDynamicFeatureRoutes(string spaDirectory, string entityName, string entityPlural, IFileSystem fileSystem)
	{
		var routesIndexClassPath = ClassPathHelper.BffSpaFeatureClassPath(spaDirectory, entityName, BffFeatureCategory.Routes , "index.ts");
		var routesIndexFileText = GetAuthFeatureRoutesIndexText(entityName);
		Utilities.CreateFile(routesIndexClassPath, routesIndexFileText, fileSystem);

		var routesLoginFileText = GetEntityListRouteText(entityName, entityPlural);
		var routesLoginClassPath = ClassPathHelper.BffSpaFeatureClassPath(spaDirectory, 
			entityName, 
			BffFeatureCategory.Routes , 
			$"{entityName.UppercaseFirstLetter()}List.tsx");
		Utilities.CreateFile(routesLoginClassPath, routesLoginFileText, fileSystem);
	}
	
	public static string GetAuthFeatureRoutesIndexText(string entityName)
	{
	    return @$"export * from './{entityName.UppercaseFirstLetter()}List';";
	}

public static string GetEntityListRouteText(string entityName, string entityPlural)
{
	var actualDataVar = $"{entityName.LowercaseFirstLetter()}Data";
    return @$"import React from 'react';
import {{ use{entityPlural.UppercaseFirstLetter()} }} from '../api';

function {entityName.UppercaseFirstLetter()}List() {{
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

export {{ {entityName.UppercaseFirstLetter()}List }};
";
	}
}
