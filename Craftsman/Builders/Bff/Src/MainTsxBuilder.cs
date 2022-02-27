namespace Craftsman.Builders.Bff.Src
{
	using System.IO.Abstractions;
	using Helpers;

	public class MainTsxBuilder
  {
    public static void CreateMainTsx(string spaDirectory, IFileSystem fileSystem)
    {
      var classPath = ClassPathHelper.BffSpaSrcClassPath(spaDirectory, "main.tsx");
      var fileText = GetMainTsxText();
      Utilities.CreateFile(classPath, fileText, fileSystem);
    }

    public static string GetMainTsxText()
    {
      return @$"import '@/custom.css';
import React from 'react';
import ReactDOM from 'react-dom';
import App from './App';
import {{ QueryClient, QueryClientProvider }} from 'react-query';
import {{ ReactQueryDevtools }} from 'react-query/devtools';
import {{ Notifications }} from '@/components/Notifications';
import 'react-toastify/dist/ReactToastify.min.css';

ReactDOM.render(
	<React.StrictMode>
		<QueryClientProvider client={{new QueryClient()}}>
			<App />
			<ReactQueryDevtools initialIsOpen={{false}} />
		</QueryClientProvider>
		<Notifications />
	</React.StrictMode>,
	document.getElementById('root')
);";
    }
  }
}