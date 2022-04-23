namespace Craftsman.Builders.Bff.Features.Auth;

using Domain.Enums;
using Helpers;
using Services;

public class AuthFeatureRoutesBuilder
{
	private readonly ICraftsmanUtilities _utilities;

	public AuthFeatureRoutesBuilder(ICraftsmanUtilities utilities)
	{
		_utilities = utilities;
	}

	public void CreateAuthFeatureRoutes(string spaDirectory)
	{
		var routesIndexClassPath = ClassPathHelper.BffSpaFeatureClassPath(spaDirectory, "Auth", BffFeatureCategory.Routes , "index.ts");
		var routesIndexFileText = GetAuthFeatureRoutesIndexText();
		_utilities.CreateFile(routesIndexClassPath, routesIndexFileText);

		var routesLoginClassPath = ClassPathHelper.BffSpaFeatureClassPath(spaDirectory, "Auth", BffFeatureCategory.Routes , "Login.tsx");
		var routesLoginFileText = GetAuthFeatureRoutesLoginText();
		_utilities.CreateFile(routesLoginClassPath, routesLoginFileText);
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
	const {{ isLoading }} = useAuthUser();

	return (
		<div className='font-sans antialiased bg-gray-900 text-white min-h-screen flex flex-col items-center justify-center p-8 text-sm sm:text-base transition-colors ease-out'>
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
				<div className='grid grid-cols-3 gap-4 md:gap-8 max-w-5xl w-full z-20'>
					<div className='flex justify-between items-end col-span-3'>
						<a href='https://wrapt.dev' target='_blank' rel='noopener' className='wrapt-logo'>
							<svg xmlns='http://www.w3.org/2000/svg' viewBox='0 0 100 18.465' className='h-12 w-auto'>
								<g fill='#7A3ED7'>
									<g fill='#fff'>
										<path d='M29.574 12.368l-2.325-6.644h-1.677l3.223 9.233h1.545l1.942-6.327h.105l1.942 6.327h1.545l3.223-9.233H37.42l-2.325 6.644-2.06-6.644h-1.4zM54.088 8.722c0-1.047-.303-1.81-.911-2.285-.608-.475-1.616-.713-3.025-.713h-3.434v9.233h1.559v-3.104h2.047l2.206 3.104h1.981l-2.417-3.355c1.33-.396 1.994-1.356 1.994-2.88zm-2.073 1.308c-.317.273-.916.41-1.797.41h-1.941V7.136h1.981c.836 0 1.418.113 1.743.337.326.225.49.639.49 1.242s-.16 1.041-.476 1.314zM63.823 12.857h4.623l.924 2.1h1.664l-4.068-9.233h-1.664l-4.068 9.233h1.664zm3.989-1.44h-3.355l1.677-3.804zM84.731 6.53c-.642-.537-1.664-.806-3.064-.806h-3.17v9.233h1.559v-2.708h1.637c1.356 0 2.363-.26 3.019-.78.656-.519.984-1.338.984-2.456 0-1.118-.322-1.946-.965-2.483zm-1.103 3.81c-.312.33-.9.496-1.763.496h-1.81V7.137h1.586c.845 0 1.466.133 1.862.397.396.264.594.715.594 1.354 0 .638-.156 1.122-.469 1.452zM97.2 7.15h2.8V5.725h-7.159v1.427h2.8v7.806H97.2z' />
									</g>
									<path d='M16.272 13.849a1.847 1.847 0 01-3.18.02 5.252 5.252 0 00-1.17-1.354c-.308-.205-.616-.41-.944-.615a9.114 9.114 0 00-1.005-.493c-.185-.082-.37-.123-.554-.184-.041.184-.082.369-.123.574-.062.37-.082.739-.082 1.129s.02.738.082 1.107c.082.595.287 1.17.574 1.683.144.267.226.554.226.882.02 1.026-.82 1.867-1.847 1.867s-1.867-.841-1.846-1.867c0-.308.082-.615.226-.882a4.815 4.815 0 00.574-1.683c.062-.369.082-.738.082-1.107 0-.37-.02-.74-.082-1.129a2.875 2.875 0 00-.123-.574c-.185.04-.37.102-.554.184a9.114 9.114 0 00-1.005.493c-.329.184-.636.39-.923.636-.472.369-.862.84-1.17 1.354-.144.246-.37.472-.636.636-.882.513-2.031.226-2.544-.657a1.831 1.831 0 01.698-2.523c.266-.164.574-.226.861-.246a4.64 4.64 0 001.744-.35 9.114 9.114 0 001.005-.492c.329-.184.636-.41.924-.636.43-.328.78-.738 1.067-1.19l.061-.123c.02-.02.02-.04.041-.061a4.798 4.798 0 00.534-1.58c.061-.37.082-.739.082-1.108 0-.37-.02-.739-.082-1.108a4.815 4.815 0 00-.575-1.682c-.102-.308-.185-.595-.185-.903C6.403.841 7.244 0 8.27 0s1.867.841 1.847 1.867c0 .308-.083.616-.226.882a4.815 4.815 0 00-.575 1.683c-.061.369-.082.738-.082 1.108 0 .369.02.738.082 1.107.082.575.267 1.108.554 1.621 0 .02.02.02.02.041l.062.123a4.93 4.93 0 001.067 1.19c.287.226.595.452.923.636.329.185.657.35 1.006.493a5.078 5.078 0 001.744.349c.307 0 .595.082.861.246a1.81 1.81 0 01.719 2.503z' />
								</g>
							</svg>
						</a>
						<a href='https://github.com/pdevito3/craftsman' target='_blank' rel='noopener' className='flex justify-end pb-1 sm:pb-2' />
					</div>
					<div className='col-span-3 rounded-md p-4 flex flex-col space-y-6 sm:space-y-4'>
						<div className='flex justify-between items-center'>
							<h4 className='font-medium text-2xl'>Get Started</h4>
						</div>
						<p>Login to your auth server to access the private pages. Make sure your Auth Server and API projects are also running.</p>
						<a
							href='/bff/login?returnUrl=/'
							className='text-center rounded-md p-4 sm:p-2 border border-gray sm:w-36 hover:bg-gray-800 transition-colors ease-in-out'
						>
							Login
						</a>
					</div>
					<a href='https://wrapt.dev' target='_blank' rel='noopener' className='border border-transparent rounded-md transition-colors ease-in cursor-pointer col-span-3 sm:col-span-1 p-4 flex flex-col hover:border-gray-600 hover:bg-gray-800'><svg width={{40}} height={{40}} viewBox='0 0 40 40' fill='none' xmlns='http://www.w3.org/2000/svg'>
						<path d='M20 10.4217C21.9467 9.12833 24.59 8.33333 27.5 8.33333C30.4117 8.33333 33.0533 9.12833 35 10.4217V32.0883C33.0533 30.795 30.4117 30 27.5 30C24.59 30 21.9467 30.795 20 32.0883M20 10.4217V32.0883V10.4217ZM20 10.4217C18.0533 9.12833 15.41 8.33333 12.5 8.33333C9.59 8.33333 6.94667 9.12833 5 10.4217V32.0883C6.94667 30.795 9.59 30 12.5 30C15.41 30 18.0533 30.795 20 32.0883V10.4217Z' stroke='currentColor' strokeWidth={{2}} strokeLinecap='round' strokeLinejoin='round' />
						<rect x='23.3334' y='13.3333' width='8.33334' height='1.66667' rx='0.833333' fill='currentColor' />
						<rect x='8.33337' y='13.3333' width='8.33333' height='1.66667' rx='0.833333' fill='currentColor' />
						<rect x='8.33337' y='18.3333' width='8.33333' height='1.66667' rx='0.833333' fill='currentColor' />
						<rect x='8.33337' y='23.3333' width='8.33333' height='1.66667' rx='0.833334' fill='currentColor' />
						<rect x='23.3334' y='18.3333' width='8.33334' height='1.66667' rx='0.833333' fill='currentColor' />
						<rect x='23.3334' y='23.3333' width='8.33334' height='1.66667' rx='0.833334' fill='currentColor' />
					</svg>
						<h5 className='font-semibold text-xl mt-4'>Documentation</h5>
						<p className='mt-2'>Find in depth information about the Wrapt framework.</p>
					</a>
					<a href='https://github.com/pdevito3/craftsman' target='_blank' rel='noopener' className='cursor-pointer border border-transparent rounded-md transition-colors ease-in col-span-3 sm:col-span-1 p-4 flex flex-col hover:border-gray-600 hover:bg-gray-800'>
						<svg width={{40}} height={{40}} viewBox='0 0 40 40' fill='none' xmlns='http://www.w3.org/2000/svg'>
							<path fillRule='evenodd' clipRule='evenodd' d='M20 3.33333C10.795 3.33333 3.33337 10.8067 3.33337 20.0283C3.33337 27.4033 8.10837 33.6617 14.7317 35.8683C15.565 36.0217 15.8684 35.5067 15.8684 35.0633C15.8684 34.6683 15.855 33.6167 15.8467 32.225C11.21 33.2333 10.2317 29.9867 10.2317 29.9867C9.47504 28.0567 8.38171 27.5433 8.38171 27.5433C6.86837 26.51 8.49671 26.53 8.49671 26.53C10.1684 26.6467 11.0484 28.25 11.0484 28.25C12.535 30.8 14.95 30.0633 15.8984 29.6367C16.0517 28.5583 16.4817 27.8233 16.9584 27.4067C13.2584 26.985 9.36671 25.5517 9.36671 19.155C9.36671 17.3333 10.0167 15.8417 11.0817 14.675C10.91 14.2533 10.3384 12.555 11.245 10.2583C11.245 10.2583 12.645 9.80833 15.8284 11.9683C17.188 11.5975 18.5908 11.4087 20 11.4067C21.4167 11.4133 22.8417 11.5983 24.1734 11.9683C27.355 9.80833 28.7517 10.2567 28.7517 10.2567C29.6617 12.555 29.0884 14.2533 28.9184 14.675C29.985 15.8417 30.6317 17.3333 30.6317 19.155C30.6317 25.5683 26.7334 26.98 23.0217 27.3933C23.62 27.9083 24.1517 28.9267 24.1517 30.485C24.1517 32.715 24.1317 34.5167 24.1317 35.0633C24.1317 35.51 24.4317 36.03 25.2784 35.8667C28.5972 34.7535 31.4823 32.6255 33.5258 29.7834C35.5694 26.9413 36.6681 23.5289 36.6667 20.0283C36.6667 10.8067 29.2034 3.33333 20 3.33333Z' fill='currentColor' />
						</svg>
						<h5 className='font-semibold text-xl mt-4'>GitHub</h5>
						<p className='mt-2'>Powered by an OS project called Craftsman, all the code available on GitHub. Stars ⭐️ are greatly appreciated!</p>
					</a>
					<a href='https://discord.gg/TBq2rVkSEj' target='_blank' rel='noopener' className='cursor-pointer border border-transparent rounded-md transition-colors ease-in col-span-3 sm:col-span-1 p-4 flex flex-col gap-y-4 hover:border-gray-600 hover:bg-gray-800'>
						<svg className='w-10 h-10' width={{71}} height={{55}} viewBox='0 0 71 55' fill='none' xmlns='http://www.w3.org/2000/svg'>
							<g clipPath='url(#clip0)'>
								<path d='M60.1045 4.8978C55.5792 2.8214 50.7265 1.2916 45.6527 0.41542C45.5603 0.39851 45.468 0.440769 45.4204 0.525289C44.7963 1.6353 44.105 3.0834 43.6209 4.2216C38.1637 3.4046 32.7345 3.4046 27.3892 4.2216C26.905 3.0581 26.1886 1.6353 25.5617 0.525289C25.5141 0.443589 25.4218 0.40133 25.3294 0.41542C20.2584 1.2888 15.4057 2.8186 10.8776 4.8978C10.8384 4.9147 10.8048 4.9429 10.7825 4.9795C1.57795 18.7309 -0.943561 32.1443 0.293408 45.3914C0.299005 45.4562 0.335386 45.5182 0.385761 45.5576C6.45866 50.0174 12.3413 52.7249 18.1147 54.5195C18.2071 54.5477 18.305 54.5139 18.3638 54.4378C19.7295 52.5728 20.9469 50.6063 21.9907 48.5383C22.0523 48.4172 21.9935 48.2735 21.8676 48.2256C19.9366 47.4931 18.0979 46.6 16.3292 45.5858C16.1893 45.5041 16.1781 45.304 16.3068 45.2082C16.679 44.9293 17.0513 44.6391 17.4067 44.3461C17.471 44.2926 17.5606 44.2813 17.6362 44.3151C29.2558 49.6202 41.8354 49.6202 53.3179 44.3151C53.3935 44.2785 53.4831 44.2898 53.5502 44.3433C53.9057 44.6363 54.2779 44.9293 54.6529 45.2082C54.7816 45.304 54.7732 45.5041 54.6333 45.5858C52.8646 46.6197 51.0259 47.4931 49.0921 48.2228C48.9662 48.2707 48.9102 48.4172 48.9718 48.5383C50.038 50.6034 51.2554 52.5699 52.5959 54.435C52.6519 54.5139 52.7526 54.5477 52.845 54.5195C58.6464 52.7249 64.529 50.0174 70.6019 45.5576C70.6551 45.5182 70.6887 45.459 70.6943 45.3942C72.1747 30.0791 68.2147 16.7757 60.1968 4.9823C60.1772 4.9429 60.1437 4.9147 60.1045 4.8978ZM23.7259 37.3253C20.2276 37.3253 17.3451 34.1136 17.3451 30.1693C17.3451 26.225 20.1717 23.0133 23.7259 23.0133C27.308 23.0133 30.1626 26.2532 30.1066 30.1693C30.1066 34.1136 27.28 37.3253 23.7259 37.3253ZM47.3178 37.3253C43.8196 37.3253 40.9371 34.1136 40.9371 30.1693C40.9371 26.225 43.7636 23.0133 47.3178 23.0133C50.9 23.0133 53.7545 26.2532 53.6986 30.1693C53.6986 34.1136 50.9 37.3253 47.3178 37.3253Z' fill='#ffffff' />
							</g>
							<defs>
								<clipPath id='clip0'>
									<rect width={{71}} height={{55}} fill='white' />
								</clipPath>
							</defs>
						</svg>
						<h5 className='font-semibold text-xl'>Discord</h5>
						<p>Join the Wrapt Discord to participate in community discussions and get latest news about releases.</p>
					</a>
				</div>
			)}}
		</div>
	);
}}

export {{ Login }};
";
	}
}
