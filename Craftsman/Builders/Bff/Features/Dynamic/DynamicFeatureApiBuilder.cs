namespace Craftsman.Builders.Bff.Features.Dynamic;

using System.IO.Abstractions;
using Enums;
using Helpers;

public class DynamicFeatureApiBuilder
{
	public static void CreateDynamicFeatureApis(string spaDirectory, IFileSystem fileSystem)
	{
		var routesIndexClassPath = ClassPathHelper.BffSpaFeatureClassPath(spaDirectory, "Dynamic", BffFeatureCategory.Api , "index.ts");
		var routesIndexFileText = GetDynamicFeatureApisIndexText();
		Utilities.CreateFile(routesIndexClassPath, routesIndexFileText, fileSystem);

		var routesLoginClassPath = ClassPathHelper.BffSpaFeatureClassPath(spaDirectory, "Dynamic", BffFeatureCategory.Api , "Login.tsx");
		var routesLoginFileText = GetDynamicFeatureApisLoginText();
		Utilities.CreateFile(routesLoginClassPath, routesLoginFileText, fileSystem);
	}
	
	public static string GetDynamicFeatureApisIndexText()
	{
	    return @$"export * from './useDynamicUser';";
	}

public static string GetDynamicFeatureApisLoginText()
{
    return @$"import {{ api }} from '@/lib/axios';
import {{ useEffect, useState }} from 'react';
import {{ useQuery }} from 'react-query';

const claimsKeys = {{
	claim: ['claims'],
}};

const fetchClaims = async () => api.get('/bff/user').then((res) => res.data);

function useClaims() {{
	return useQuery(
		claimsKeys.claim,
		async () => {{
			return fetchClaims();
		}},
	);
}}

function useDynamicUser() {{
	const {{ data: claims, isLoading }} = useClaims();

	let logoutUrl = claims?.find((claim: any) => claim.type === 'bff:logout_url');
	let nameDict =
		claims?.find((claim: any) => claim.type === 'name') ||
		claims?.find((claim: any) => claim.type === 'sub');
	let username = nameDict?.value;

	const [isLoggedIn, setIsLoggedIn] = useState(false);
	useEffect(() => {{
		setIsLoggedIn(!!username);
	}}, [username]);

	return {{
		username,
		logoutUrl,
		isLoading,
		isLoggedIn,
	}};
}}

export {{ useDynamicUser }};
";
	}
}
