namespace Craftsman.Builders.Bff.Components.Layouts;

using System.IO.Abstractions;
using Helpers;

public class LayoutComponentBuilder
{
    public static void CreateLayoutComponentItems(string spaDirectory, IFileSystem fileSystem)
    {
      var indexCassPath = ClassPathHelper.BffSpaComponentClassPath(spaDirectory, "Layouts", "index.ts");
      var indexFileText = GetLayoutIndexText();
      Utilities.CreateFile(indexCassPath, indexFileText, fileSystem);
      
      var privateLayoutClassPath = ClassPathHelper.BffSpaComponentClassPath(spaDirectory, "Layouts", "PrivateLayout.tsx");
      var privateLayoutFileText = GetPrivateLayoutText();
      Utilities.CreateFile(privateLayoutClassPath, privateLayoutFileText, fileSystem);
      
      var publicLayoutClassPath = ClassPathHelper.BffSpaComponentClassPath(spaDirectory, "Layouts", "PublicLayout.tsx");
      var publicLayoutFileText = GetPublicLayoutText();
      Utilities.CreateFile(publicLayoutClassPath, publicLayoutFileText, fileSystem);
    }

    public static string GetLayoutIndexText()
    {
      return @$"export * from './PublicLayout';
export * from './PrivateLayout';";
    }
	
    public static string GetPrivateLayoutText()
    {
        return @$"import {{ useAuthUser }} from '@/features/Auth';
import React from 'react';
import {{ Outlet }} from 'react-router';
import {{ PrivateHeader }} from '../Headers';
import {{ PrivateSideNav }} from '../Navigation';

function PrivateLayout() {{
	const {{ username }} = useAuthUser();

	return (
		<div className='flex w-full h-full'>
			<PrivateSideNav />
			<div className='w-full h-full'>
				<PrivateHeader />
				<main className='flex-1 h-full p-4 bg-gray-50'>
					<div className=''>
						{{!username ? (
							<a
								href='/bff/login?returnUrl=/'
								className='inline-block px-4 py-2 text-base font-medium text-center text-white bg-blue-500 border border-transparent rounded-md hover:bg-opacity-75'
							>
								Login
							</a>
						) : null}}
					</div>
					<Outlet />
				</main>
			</div>
		</div>
	);
}}

export {{ PrivateLayout }};";
    }
	
    public static string GetPublicLayoutText()
    {
	    return @$"import React from 'react';
import {{ Outlet }} from 'react-router';

function PublicLayout() {{
	return (
		<div>
			<Outlet />
		</div>
	);
}}

export {{ PublicLayout }};";
    }
}
