namespace Craftsman.Builders.Projects
{
    using System;
    using Craftsman.Exceptions;
    using Craftsman.Helpers;
    using System.IO;
    using System.IO.Abstractions;
    using System.Text;
    using Enums;

    public class BffProjBuilder
    {
        public static void CreateProject(string solutionDirectory, string projectBaseName, int? proxyPort, IFileSystem fileSystem)
        {
            var classPath = ClassPathHelper.WebApiProjectClassPath(solutionDirectory, projectBaseName);
            var fileText = ProjectFileText(proxyPort);
            Utilities.CreateFile(classPath, fileText, fileSystem);
        }

        public static string ProjectFileText(int? proxyPort)
        {
            return @$"<Project Sdk=""Microsoft.NET.Sdk.Web"">

  <PropertyGroup>
    <TargetFramework>net6.0</TargetFramework>
    <Nullable>enable</Nullable>
    <TypeScriptCompileBlocked>true</TypeScriptCompileBlocked>
    <TypeScriptToolsVersion>Latest</TypeScriptToolsVersion>
    <IsPackable>false</IsPackable>
    <SpaRoot>ClientApp\</SpaRoot>
    <DefaultItemExcludes>$(DefaultItemExcludes);$(SpaRoot)node_modules\**</DefaultItemExcludes>
    <SpaProxyServerUrl>https://localhost:{proxyPort}</SpaProxyServerUrl>
    <SpaProxyLaunchCommand>yarn start</SpaProxyLaunchCommand>
    <RootNamespace>react_six</RootNamespace>
    <ImplicitUsings>enable</ImplicitUsings>
  </PropertyGroup>

  <ItemGroup>
    <PackageReference Include=""Duende.BFF"" Version=""1.1.3"" />
    <PackageReference Include=""Duende.BFF.Yarp"" Version=""1.1.3"" />
    <PackageReference Include=""Microsoft.AspNetCore.SpaProxy"" Version=""6.0.0"" />
    <PackageReference Include=""Microsoft.AspNetCore.Authentication.OpenIdConnect"" Version=""6.0.0"" />
  </ItemGroup>

  <ItemGroup>
    <!-- Don't publish the SPA source files, but do show them in the project files list -->
    <Content Remove=""$(SpaRoot)**"" />
    <None Remove=""$(SpaRoot)**"" />
    <None Include=""$(SpaRoot)**"" Exclude=""$(SpaRoot)node_modules\**"" />
  </ItemGroup>

  <ItemGroup>
    <_ContentIncludedByDefault Remove=""Pages\Error.cshtml"" />
    <_ContentIncludedByDefault Remove=""Pages\_ViewImports.cshtml"" />
  </ItemGroup>

  <Target Name=""DebugEnsureNodeEnv"" BeforeTargets=""Build"" Condition="" '$(Configuration)' == 'Debug' And !Exists('$(SpaRoot)node_modules') "">
    <!-- Ensure Node.js is installed -->
    <Exec Command=""node --version"" ContinueOnError=""true"">
      <Output TaskParameter=""ExitCode"" PropertyName=""ErrorCode"" />
    </Exec>
    <Error Condition=""'$(ErrorCode)' != '0'"" Text=""Node.js is required to build and run this project. To continue, please install Node.js from https://nodejs.org/, and then restart your command prompt or IDE."" />
    <Message Importance=""high"" Text=""Restoring dependencies using 'yarn'. This may take several minutes..."" />
    <Exec WorkingDirectory=""$(SpaRoot)"" Command=""yarn install"" />
  </Target>

  <Target Name=""PublishRunWebpack"" AfterTargets=""ComputeFilesToPublish"">
    <!-- As part of publishing, ensure the JS resources are freshly built in production mode -->
    <Exec WorkingDirectory=""$(SpaRoot)"" Command=""yarn install"" />
    <Exec WorkingDirectory=""$(SpaRoot)"" Command=""yarn run build"" />

    <!-- Include the newly-built files in the publish output -->
    <ItemGroup>
      <DistFiles Include=""$(SpaRoot)build\**"" />
      <ResolvedFileToPublish Include=""@(DistFiles->'%(FullPath)')"" Exclude=""@(ResolvedFileToPublish)"">
        <RelativePath>wwwroot\%(RecursiveDir)%(FileName)%(Extension)</RelativePath>
        <CopyToPublishDirectory>PreserveNewest</CopyToPublishDirectory>
        <ExcludeFromSingleFile>true</ExcludeFromSingleFile>
      </ResolvedFileToPublish>
    </ItemGroup>
  </Target>
</Project>
";
        }
    }
}