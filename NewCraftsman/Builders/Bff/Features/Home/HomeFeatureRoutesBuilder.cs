namespace NewCraftsman.Builders.Bff.Features.Home;

using System.IO.Abstractions;
using Domain.Enums;
using Helpers;
using Services;

public class HomeFeatureRoutesBuilder
{
	private readonly ICraftsmanUtilities _utilities;

	public HomeFeatureRoutesBuilder(ICraftsmanUtilities utilities)
	{
		_utilities = utilities;
	}

	public void CreateHomeFeatureRoutes(string spaDirectory)
	{
		var routesIndexClassPath = ClassPathHelper.BffSpaFeatureClassPath(spaDirectory, 
			"Home", 
			BffFeatureCategory.Routes , 
			"index.ts");
		var routesIndexFileText = GetHomeFeatureRoutesIndexText();
		_utilities.CreateFile(routesIndexClassPath, routesIndexFileText);

		var routesLoginClassPath = ClassPathHelper.BffSpaFeatureClassPath(spaDirectory, 
			"Home", 
			BffFeatureCategory.Routes , 
			"Home.tsx");
		var routesLoginFileText = GetHomeFeatureRoutesLoginText();
		_utilities.CreateFile(routesLoginClassPath, routesLoginFileText);
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
