namespace Craftsman.Builders.Bff
{
  using System;
  using System.IO.Abstractions;
  using System.Linq;
  using Enums;
  using Helpers;
  using Models;
  using static Helpers.ConstMessages;

  public class AspnetcoreHttpsBuilder
  {
    public static void CreateAspnetcoreHttps(string spaDirectory, IFileSystem fileSystem)
    {
      var classPath = ClassPathHelper.BffSpaRootClassPath(spaDirectory, "aspnetcore-https.js");
      var fileText = GetAspnetcoreHttpsText();
      Utilities.CreateFile(classPath, fileText, fileSystem);
    }

    public static string GetAspnetcoreHttpsText()
    {
      return @$"// This script sets up HTTPS for the application using the ASP.NET Core HTTPS certificate
const fs = require('fs');
const spawn = require('child_process').spawn;
const path = require('path');

const baseFolder =
	process.env.APPDATA !== undefined && process.env.APPDATA !== ''
		? `${{process.env.APPDATA}}/ASP.NET/https`
		: `${{process.env.HOME}}/.aspnet/https`;

const certificateArg = process.argv
	.map((arg) => arg.match(/--name=(?<value>.+)/i))
	.filter(Boolean)[0];
const certificateName = certificateArg ? certificateArg.groups.value : process.env.npm_package_name;

if (!certificateName) {{
	console.error(
		'Invalid certificate name. Run this script in the context of an npm/yarn script or pass --name=<<app>> explicitly.'
	);
	process.exit(-1);
}}

const certFilePath = path.join(baseFolder, `${{certificateName}}.pem`);
const keyFilePath = path.join(baseFolder, `${{certificateName}}.key`);

if (!fs.existsSync(certFilePath) || !fs.existsSync(keyFilePath)) {{
	spawn(
		'dotnet',
		['dev-certs', 'https', '--export-path', certFilePath, '--format', 'Pem', '--no-password'],
		{{ stdio: 'inherit' }}
	).on('exit', (code) => process.exit(code));
}}
";
    }
  }
}