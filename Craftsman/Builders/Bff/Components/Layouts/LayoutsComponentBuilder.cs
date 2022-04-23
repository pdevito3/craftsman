namespace Craftsman.Builders.Bff.Components.Layouts;

using Helpers;
using Services;

public class LayoutComponentBuilder
{
	private readonly ICraftsmanUtilities _utilities;

	public LayoutComponentBuilder(ICraftsmanUtilities utilities)
	{
		_utilities = utilities;
	}

    public void CreateLayoutComponentItems(string spaDirectory)
    {
      var indexCassPath = ClassPathHelper.BffSpaComponentClassPath(spaDirectory, "Layouts", "index.ts");
      var indexFileText = GetLayoutIndexText();
      _utilities.CreateFile(indexCassPath, indexFileText);
      
      var privateLayoutClassPath = ClassPathHelper.BffSpaComponentClassPath(spaDirectory, "Layouts", "PrivateLayout.tsx");
      var privateLayoutFileText = GetPrivateLayoutText();
      _utilities.CreateFile(privateLayoutClassPath, privateLayoutFileText);
      
      var publicLayoutClassPath = ClassPathHelper.BffSpaComponentClassPath(spaDirectory, "Layouts", "PublicLayout.tsx");
      var publicLayoutFileText = GetPublicLayoutText();
      _utilities.CreateFile(publicLayoutClassPath, publicLayoutFileText);
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
