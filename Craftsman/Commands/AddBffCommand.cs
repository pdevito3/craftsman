namespace Craftsman.Commands
{
    using Craftsman.Exceptions;
    using Craftsman.Helpers;
    using Craftsman.Models;
    using System;
    using System.Collections.Generic;
    using System.IO;
    using System.IO.Abstractions;
    using Builders;
    using Builders.Bff;
    using Builders.Bff.Components.Headers;
    using Builders.Bff.Components.Layouts;
    using Builders.Bff.Components.Navigation;
    using Builders.Bff.Components.Notifications;
    using Builders.Bff.Features.Auth;
    using Builders.Bff.Features.Dynamic;
    using Builders.Bff.Features.Dynamic.Api;
    using Builders.Bff.Features.Dynamic.Types;
    using Builders.Bff.Features.Home;
    using Builders.Bff.Src;
    using static Helpers.ConsoleWriter;
    using Spectre.Console;
    using Craftsman.Builders.Tests.Utilities;
    using Enums;
    using AppSettingsBuilder = Builders.Bff.AppSettingsBuilder;
    using ProgramBuilder = Builders.Bff.ProgramBuilder;

    public static class AddBffCommand
    {
        public static void Help()
        {
            WriteHelpHeader(@$"Description:");
            WriteHelpText(@$"   This command will add a bff to your solution along with a React client using a formatted yaml or json file that describes the bff you want to add.{Environment.NewLine}");

            WriteHelpHeader(@$"Usage:");
            WriteHelpText(@$"   craftsman add:bff [options] <filepath>");

            WriteHelpText(Environment.NewLine);
            WriteHelpHeader(@$"Arguments:");
            WriteHelpText(@$"   filepath         The full filepath for the yaml or json file that lists the bff information that you want to add to your solution.");

            WriteHelpText(Environment.NewLine);
            WriteHelpHeader(@$"Options:");
            WriteHelpText(@$"   -h, --help          Display this help message. No filepath is needed to display the help message.");

            WriteHelpText(Environment.NewLine);
            WriteHelpHeader(@$"Example:");
            WriteHelpText(@$"   craftsman add:bff C:\fullpath\mybffinfo.yaml");
            WriteHelpText(@$"   craftsman add:bff C:\fullpath\mybffinfo.yml");
            WriteHelpText(@$"   craftsman add:bff C:\fullpath\mybffinfo.json");
        }

        public static void Run(string filePath, string domainDirectory, IFileSystem fileSystem)
        {
            try
            {
                FileParsingHelper.RunInitialTemplateParsingGuards(filePath);
                Utilities.SolutionGuard(domainDirectory);

                var template = FileParsingHelper.GetTemplateFromFile<BffTemplate>(filePath);
                WriteHelpText($"Your bff template file was parsed successfully.");

                AnsiConsole.Status()
                    .AutoRefresh(true)
                    .Spinner(Spinner.Known.Dots2)
                    .Start($"[yellow]Creating {template.ProjectName} [/]", ctx =>
                    {
                        AddBff(template, domainDirectory, fileSystem);
                        
                        WriteLogMessage($"File scaffolding for {template.ProjectName} was successful");
                    });
                
                WriteHelpHeader($"{Environment.NewLine}Your bff has been successfully added. Keep up the good work!");
                StarGithubRequest();
            }
            catch (Exception e)
            {
                if (e is InvalidMessageBrokerException
                    || e is IsNotBoundedContextDirectory)
                {
                    WriteError($"{e.Message}");
                }
                else
                {
                    AnsiConsole.WriteException(e, new ExceptionSettings
                    {
                        Format = ExceptionFormats.ShortenEverything | ExceptionFormats.ShowLinks,
                        Style = new ExceptionStyle
                        {
                            Exception = new Style().Foreground(Color.Grey),
                            Message = new Style().Foreground(Color.White),
                            NonEmphasized = new Style().Foreground(Color.Cornsilk1),
                            Parenthesis = new Style().Foreground(Color.Cornsilk1),
                            Method = new Style().Foreground(Color.Red),
                            ParameterName = new Style().Foreground(Color.Cornsilk1),
                            ParameterType = new Style().Foreground(Color.Red),
                            Path = new Style().Foreground(Color.Red),
                            LineNumber = new Style().Foreground(Color.Cornsilk1),
                        }
                    });
                }
            }
        }

        public static void AddBff(BffTemplate template, string domainDirectory, IFileSystem fileSystem)
        {
            var projectName = template.ProjectName;
            var projectDirectory = template.GetProjectDirectory(domainDirectory);
            var spaDirectory = template.GetSpaDirectory(domainDirectory);

            SolutionBuilder.BuildBffProject(domainDirectory, projectName, template.ProxyPort, fileSystem);
            fileSystem.Directory.CreateDirectory(spaDirectory);

            // .NET Project
            LaunchSettingsBuilder.CreateLaunchSettings(projectDirectory, projectName, template, fileSystem);
            AppSettingsBuilder.CreateBffAppSettings(projectDirectory, projectName, fileSystem);
            LoggingConfigurationBuilder.CreateBffConfigFile(domainDirectory, projectName, fileSystem);
            
            ProgramBuilder.CreateProgram(projectDirectory, domainDirectory, projectName, template, fileSystem);
            
            // TODO README at root
            
            // SPA - root
            ViteConfigBuilder.CreateViteConfig(spaDirectory, template.ProxyPort, fileSystem);
            TsConfigBuilder.CreateTsConfigPaths(spaDirectory, fileSystem);
            TsConfigBuilder.CreateTsConfig(spaDirectory, fileSystem);
            TailwindConfigBuilder.CreateTailwindConfig(spaDirectory, fileSystem);
            PostCssBuilder.CreatePostCss(spaDirectory, fileSystem);
            PackageJsonBuilder.CreatePackageJson(spaDirectory, projectName, fileSystem);
            IndexHtmlBuilder.CreateIndexHtml(spaDirectory, template.HeadTitle, fileSystem);
            AspnetcoreReactBuilder.CreateAspnetcoreReact(spaDirectory, fileSystem);
            AspnetcoreHttpsBuilder.CreateAspnetcoreHttps(spaDirectory, fileSystem);
            EnvBuilder.CreateEnv(spaDirectory, fileSystem);
            EnvBuilder.CreateDevEnv(spaDirectory, template.ProxyPort, fileSystem);
            PrettierRcBuilder.CreatePrettierRc(spaDirectory, fileSystem);
            
            // SPA - src
            AssetsBuilder.CreateFavicon(spaDirectory, fileSystem);
            AssetsBuilder.CreateLogo(spaDirectory, fileSystem);
            LibBuilder.CreateAxios(spaDirectory, fileSystem);
            TypesBuilder.CreateApiTypes(spaDirectory, fileSystem);
            ViteEnvBuilder.CreateViteEnv(spaDirectory, fileSystem);
            MainTsxBuilder.CreateMainTsx(spaDirectory, fileSystem);
            CustomCssBuilder.CreateCustomCss(spaDirectory, fileSystem);
            AppTsxBuilder.CreateAppTsx(spaDirectory, fileSystem);
            
            // SPA - src/components
            HeadersComponentBuilder.CreateHeaderComponentItems(spaDirectory, fileSystem);
            NotificationsComponentBuilder.CreateNotificationComponentItems(spaDirectory, fileSystem);
            NavigationComponentBuilder.CreateNavigationComponentItems(spaDirectory, fileSystem);
            LayoutComponentBuilder.CreateLayoutComponentItems(spaDirectory, fileSystem);
            
            // SPA - src/features
            AuthFeatureApiBuilder.CreateAuthFeatureApis(spaDirectory, fileSystem);
            AuthFeatureRoutesBuilder.CreateAuthFeatureRoutes(spaDirectory, fileSystem);
            AuthFeatureBuilder.CreateAuthFeatureIndex(spaDirectory, fileSystem);
            
            HomeFeatureRoutesBuilder.CreateHomeFeatureRoutes(spaDirectory, fileSystem);
            HomeFeatureBuilder.CreateHomeFeatureIndex(spaDirectory, fileSystem);
            
            EntityScaffolding.ScaffoldBffEntities(template.Entities, fileSystem, spaDirectory);

            // Docker
            // TODO add auth vars to docker compose
            
            // TODO docs on ApiAddress and making a resource to abstract out the baseurl and that the `ApiAddress` can be a string that incorporates that

            // TODO AnsiConsole injection for status updates
        }
    }
}