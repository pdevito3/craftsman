namespace Craftsman.Builders.Bff.Features.Home;

using System.IO.Abstractions;
using Enums;
using Helpers;

public class HomeFeatureRoutesBuilder
{
	public static void CreateHomeFeatureRoutes(string spaDirectory, IFileSystem fileSystem)
	{
		var routesIndexClassPath = ClassPathHelper.BffSpaFeatureClassPath(spaDirectory, 
			"Home", 
			BffFeatureCategory.Routes , 
			"index.ts");
		var routesIndexFileText = GetHomeFeatureRoutesIndexText();
		Utilities.CreateFile(routesIndexClassPath, routesIndexFileText, fileSystem);

		var routesLoginClassPath = ClassPathHelper.BffSpaFeatureClassPath(spaDirectory, 
			"Home", 
			BffFeatureCategory.Routes , 
			"Home.tsx");
		var routesLoginFileText = GetHomeFeatureRoutesLoginText();
		Utilities.CreateFile(routesLoginClassPath, routesLoginFileText, fileSystem);
	}
	
	public static string GetHomeFeatureRoutesIndexText()
	{
	    return @$"export * from './Home';";
	}

public static string GetHomeFeatureRoutesLoginText()
{
    return @$"import React from 'react';

function Home() {{
	return <div>Home!</div>;
}}

export {{ Home }};
";
	}
}
