namespace Craftsman.Commands
{
    using Craftsman.Exceptions;
    using Craftsman.Helpers;
    using Craftsman.Models;
    using System;
    using System.Collections.Generic;
    using System.IO;
    using System.IO.Abstractions;
    using Builders.Bff;
    using Builders.Bff.Src;
    using static Helpers.ConsoleWriter;
    using Spectre.Console;
    using Craftsman.Builders.Tests.Utilities;

    public static class AddBffCommand
    {
        public static void Help()
        {
            // ************************* TODO TBD
        }

        public static void Run(string filePath, string domainDirectory, IFileSystem fileSystem)
        {
            try
            {
                FileParsingHelper.RunInitialTemplateParsingGuards(filePath);
                Utilities.SolutionGuard(domainDirectory);

                var template = FileParsingHelper.GetTemplateFromFile<BffTemplate>(filePath);
                WriteHelpText($"Your template file was parsed successfully.");

                var projectName = template.ProjectName;
                AnsiConsole.Status()
                    .AutoRefresh(true)
                    .Spinner(Spinner.Known.Dots2)
                    .Start($"[yellow]Creating {template.ProjectName} [/]", ctx =>
                    {
                        var projectDirectory = $"{domainDirectory}{Path.DirectorySeparatorChar}{projectName}";
                        var spaDirectory = Path.Combine(projectDirectory, "ClientApp");
                        fileSystem.Directory.CreateDirectory(spaDirectory);

                        ctx.Spinner(Spinner.Known.BouncingBar);
                        ctx.Status($"[bold blue]Building {projectName} Projects [/]");
                        Builders.SolutionBuilder.BuildBffProject(domainDirectory, projectName, template.ProxyPort, fileSystem);

                        // add all files based on the given template config
                        ctx.Status($"[bold blue]Scaffolding Files for {projectName} [/]");
                        AddBff(template, projectDirectory, spaDirectory, fileSystem);
                        
                        WriteLogMessage($"File scaffolding for {template.ProjectName} was successful");
                    });
                
                WriteHelpHeader($"{Environment.NewLine}Your event bus has been successfully added. Keep up the good work!");
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

        public static void AddBff(BffTemplate template, string projectDirectory, string spaDirectory, IFileSystem fileSystem)
        {
            var projectName = template.ProjectName;
            
            // .NET Project
            LaunchSettingsBuilder.CreateLaunchSettings(projectDirectory, projectName, template, fileSystem);
            AppSettingsBuilder.CreateBffAppSettings(projectDirectory, projectName, fileSystem);
            
            //TODO add logging
            ProgramBuilder.CreateProgram(projectDirectory, projectName, template, fileSystem);
            
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
            LibBuilder.CreateAxios(spaDirectory, fileSystem);
            TypesBuilder.CreateApiTypes(spaDirectory, fileSystem);


            // Docker
            // TODO add auth vars to docker compose
        }
    }
}