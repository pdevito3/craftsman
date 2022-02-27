namespace Craftsman.Builders.Bff.Components.Notifications;

using System.IO.Abstractions;
using Enums;
using Helpers;

public class NotificationsComponentBuilder
{
    public static void CreateNotificationComponentItems(string spaDirectory, IFileSystem fileSystem)
    {
      var indexCassPath = ClassPathHelper.BffSpaComponentClassPath(spaDirectory, "Notifications", "index.ts");
      var indexFileText = GetNotificationIndexText();
      Utilities.CreateFile(indexCassPath, indexFileText, fileSystem);
      
      var privateNotificationClassPath = ClassPathHelper.BffSpaComponentClassPath(spaDirectory, "Notifications", "Notifications.tsx");
      var privateNotificationFileText = GetNotificationText();
      Utilities.CreateFile(privateNotificationClassPath, privateNotificationFileText, fileSystem);
    }

    public static string GetNotificationIndexText()
    {
      return @$"export * from './Notifications';";
    }
	
    public static string GetNotificationText()
    {
        return @$"import React from 'react';
import {{ toast, ToastContainer, ToastOptions }} from 'react-toastify';

const Notifications = () => {{
	return (
		<ToastContainer position={{toast.POSITION.TOP_RIGHT}} hideProgressBar={{true}} theme='colored' />
	);
}};

Notifications.success = (message: string, options?: ToastOptions<{{}}>) => {{
	toast.success(
		<div className='mx-2'>{{message}}</div>,
		Object.assign(
			{{
				bodyClassName: 'py-3',
			}},
			options
		)
	);
}};

Notifications.error = (message: string, options?: ToastOptions<{{}}>) => {{
	toast.error(
		<div className='mx-2'>{{message}}</div>,
		Object.assign(
			{{
				bodyClassName: 'py-3',
			}},
			options
		)
	);
}};

export {{ Notifications }};
";
    }
}
