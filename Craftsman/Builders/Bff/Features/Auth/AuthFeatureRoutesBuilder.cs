namespace Craftsman.Builders.Bff.Features.Auth;

using System.IO.Abstractions;
using Enums;
using Helpers;

public class AuthFeatureRoutesBuilder
{
	public static void CreateAuthFeatureRoutes(string spaDirectory, IFileSystem fileSystem)
	{
		var routesIndexClassPath = ClassPathHelper.BffSpaFeatureClassPath(spaDirectory, "Auth", BffFeatureCategory.Routes , "index.ts");
		var routesIndexFileText = GetAuthFeatureRoutesIndexText();
		Utilities.CreateFile(routesIndexClassPath, routesIndexFileText, fileSystem);

		var routesLoginClassPath = ClassPathHelper.BffSpaFeatureClassPath(spaDirectory, "Auth", BffFeatureCategory.Routes , "Login.tsx");
		var routesLoginFileText = GetAuthFeatureRoutesLoginText();
		Utilities.CreateFile(routesLoginClassPath, routesLoginFileText, fileSystem);
	}
	
	public static string GetAuthFeatureRoutesIndexText()
	{
	    return @$"export * from './Login';";
	}

public static string GetAuthFeatureRoutesLoginText()
{
    return @$"import React from 'react';
import {{ useAuthUser }} from '@/features/Auth';

function Login() {{
	const {{ username, logoutUrl, isLoading }} = useAuthUser();

	return (
		<>
			{{isLoading ? (
				<div className='absolute top-0 left-0 z-10 flex items-center justify-center w-full h-full bg-white opacity-50'>
					{{/* TODO abstract loading spinner */}}
					<svg
						className='w-5 h-5 text-gray-800 animate-spin'
						xmlns='http://www.w3.org/2000/svg'
						fill='none'
						viewBox='0 0 24 24'
					>
						<circle
							className='opacity-25'
							cx={{12}}
							cy={{12}}
							r={{10}}
							stroke='currentColor'
							strokeWidth={{4}}
						/>
						<path
							className='opacity-75'
							fill='currentColor'
							d='M4 12a8 8 0 018-8V0C5.373 0 0 5.373 0 12h4zm2 5.291A7.962 7.962 0 014 12H0c0 3.042 1.135 5.824 3 7.938l3-2.647z'
						/>
					</svg>
				</div>
			) : (
				<div className='p-20 m-12 border rounded-md'>
					<div className=''>
						{{!username ? (
							<a
								href='/bff/login?returnUrl=/'
								className='inline-block px-4 py-2 text-base font-medium text-center text-white bg-blue-500 border border-transparent rounded-md hover:bg-opacity-75'
							>
								Login
							</a>
						) : (
							<div className='flex-shrink-0 block'>
								<div className='flex items-center'>
									<div className='ml-3'>
										<p className='block text-base font-medium text-blue-500 md:text-sm'>{{`Hi, ${{username}}!`}}</p>
										<a
											href={{logoutUrl?.value}}
											className='block mt-1 text-sm font-medium text-blue-200 hover:text-blue-500 md:text-xs'
										>
											Logout
										</a>
									</div>
								</div>
							</div>
						)}}
					</div>
				</div>
			)}}
		</>
	);
}}

export {{ Login }};
";
	}
}
