namespace Craftsman.Builders.Bff
{
  using System;
  using System.IO.Abstractions;
  using System.Linq;
  using Enums;
  using Helpers;
  using Models;
  using static Helpers.ConstMessages;

  public class PackageJsonBuilder
  {
    public static void CreatePackageJson(string spaDirectory, string projectName, IFileSystem fileSystem)
    {
      var classPath = ClassPathHelper.BffSpaRootClassPath(spaDirectory, "package.json");
      var fileText = GetPackageJsonText(projectName);
      Utilities.CreateFile(classPath, fileText, fileSystem);
    }

    public static string GetPackageJsonText(string projectName)
    {
      return @$"{{
	""name"": ""{projectName}"",
	""version"": ""0.1.0"",
	""private"": true,
	""dependencies"": {{
		""@headlessui/react"": ""^1.4.3"",
		""@heroicons/react"": ""^1.0.5"",
		""@tailwindcss/forms"": ""^0.4.0"",
		""axios"": ""^0.21.4"",
		""clsx"": ""^1.1.1"",
		""query-string"": ""^7.1.1"",
		""react"": ""^17.0.2"",
		""react-avatar"": ""^4.0.0"",
		""react-dom"": ""^17.0.2"",
		""react-hook-form"": ""^7.16.0"",
		""react-query"": ""^3.24.4"",
		""react-router"": ""^6.2.1"",
		""react-router-dom"": ""^6.2.1"",
		""react-scripts"": ""^4.0.3"",
		""react-toastify"": ""^8.1.1"",
		""vite"": ""^2.7.13"",
		""web-vitals"": ""^2.1.4""
	}},
	""devDependencies"": {{
		""@vitejs/plugin-react-refresh"": ""^1.3.6"",
		""@types/jest"": ""^27.4.0"",
		""@types/node"": ""^17.0.15"",
		""@types/react"": ""^17.0.39"",
		""@types/react-dom"": ""^17.0.11"",
		""@types/react-router"": ""^5.1.18"",
		""@types/react-router-dom"": ""^5.3.3"",
		""autoprefixer"": ""^10.4.2"",
		""postcss"": ""^8.4.6"",
		""prettier"": ""^2.5.1"",
		""prettier-plugin-tailwindcss"": ""^0.1.5"",
		""tailwindcss"": ""^3.0.19"",
		""typescript"": ""^4.5.5""
	}},
	""scripts"": {{
		""prestart"": ""node aspnetcore-https && node aspnetcore-react"",
		""start"": ""vite --debug"",
		""build"": ""tsc && vite build"",
		""serve"": ""vite preview"",
		""lint"": ""eslint ./src/""
	}},
	""eslintConfig"": {{
		""extends"": [
			""react-app""
		]
	}},
	""browserslist"": {{
		""production"": [
			"">0.2%"",
			""not dead"",
			""not op_mini all""
		],
		""development"": [
			""last 1 chrome version"",
			""last 1 firefox version"",
			""last 1 safari version""
		]
	}}
}}
;";
    }
  }
}