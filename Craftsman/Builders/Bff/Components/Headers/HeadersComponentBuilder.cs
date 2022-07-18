namespace Craftsman.Builders.Bff.Components.Headers;

using Helpers;
using Services;

public class HeadersComponentBuilder
{
    private readonly ICraftsmanUtilities _utilities;

    public HeadersComponentBuilder(ICraftsmanUtilities utilities)
    {
        _utilities = utilities;
    }

    public void CreateHeaderComponentItems(string spaDirectory)
    {
        var indexCassPath = ClassPathHelper.BffSpaComponentClassPath(spaDirectory, "Headers", "index.ts");
        var indexFileText = GetHeaderIndexText();
        _utilities.CreateFile(indexCassPath, indexFileText);

        var privateHeaderClassPath = ClassPathHelper.BffSpaComponentClassPath(spaDirectory, "Headers", "PrivateHeader.tsx");
        var privateHeaderFileText = GetPrivateHeaderText();
        _utilities.CreateFile(privateHeaderClassPath, privateHeaderFileText);
    }

    public static string GetHeaderIndexText()
    {
        return @$"export * from './PrivateHeader';";
    }

    public static string GetPrivateHeaderText()
    {
        return @$"import {{ Menu, Transition }} from '@headlessui/react';
import clsx from 'clsx';
import React, {{ Fragment }} from 'react';
import Avatar from 'react-avatar';
import {{ useAuthUser }} from '@/features/Auth';

function PrivateHeader() {{
    const {{ username, logoutUrl }} = useAuthUser();

    return (
        <nav className='relative w-full bg-white shadow-md'>
            <div className='px-2 mx-auto sm:px-6 lg:px-8'>
                <div className='relative flex items-center justify-between h-16'>
                    <div className='absolute inset-y-0 left-0 flex items-center sm:hidden'></div>
                    <div className='flex-1'></div>
                    <div className='absolute inset-y-0 right-0 flex items-center pr-2 space-x-5 sm:static sm:inset-auto sm:ml-6 sm:space-x-2 sm:pr-0'>
                        {{/* <!-- Profile dropdown --> */}}
                        <Menu as='div' className='relative ml-3'>
                            <div>
                                <Menu.Button className='flex text-sm bg-gray-800 rounded-full focus:outline-none focus:ring-2 focus:ring-white focus:ring-offset-2 focus:ring-offset-gray-800'>
                                    <span className='sr-only'>Open user menu</span>
                                    <Avatar name={{username}} round size='36' />
                                </Menu.Button>
                            </div>
                            <Transition
                                as={{Fragment}}
                                enter='transition ease-out duration-100'
                                enterFrom='transform opacity-0 scale-95'
                                enterTo='transform opacity-100 scale-100'
                                leave='transition ease-in duration-75'
                                leaveFrom='transform opacity-100 scale-100'
                                leaveTo='transform opacity-0 scale-95'
                            >
                                <Menu.Items className='absolute right-0 w-48 py-1 mt-2 origin-top-right bg-white rounded-md shadow-lg ring-1 ring-black ring-opacity-5 focus:outline-none'>
                                    <Menu.Item>
                                        {{({{ active }}) => (
                                            <a
                                                href={{logoutUrl?.value}}
                                                className={{clsx(
                                                    active ? 'bg-gray-100' : '',
                                                    'block px-4 py-2 text-sm text-gray-700'
                                                )}}
                                            >
                                                Logout
                                            </a>
                                        )}}
                                    </Menu.Item>
                                </Menu.Items>
                            </Transition>
                        </Menu>
                    </div>
                </div>
            </div>
        </nav>
    );
}}

export {{ PrivateHeader }};";
    }
}
