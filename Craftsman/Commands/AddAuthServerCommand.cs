namespace Craftsman.Commands
{
    using Craftsman.Builders;
    using Craftsman.Exceptions;
    using Craftsman.Helpers;
    using Craftsman.Models;
    using System;
    using System.Collections.Generic;
    using System.IO;
    using System.IO.Abstractions;
    using Builders.AuthServer;
    using static Helpers.ConsoleWriter;
    using Spectre.Console;
    using Craftsman.Validators;

    public static class AddAuthServerCommand
    {
        public static void Help()
        {
            
            // TODO help text!
            
            
            
            // WriteHelpHeader(@$"Description:");
            // WriteHelpText(@$"   This command will add a messageto your messages project using a formatted yaml or json file.{Environment.NewLine}");
            //
            // WriteHelpHeader(@$"Usage:");
            // WriteHelpText(@$"   craftsman add:message [options] <filepath>");
            //
            // WriteHelpText(Environment.NewLine);
            // WriteHelpHeader(@$"Arguments:");
            // WriteHelpText(@$"   filepath         The full filepath for the yaml or json file that lists the message information that you want to add to your project.");
            //
            // WriteHelpText(Environment.NewLine);
            // WriteHelpHeader(@$"Options:");
            // WriteHelpText(@$"   -h, --help          Display this help message. No filepath is needed to display the help message.");
            //
            // WriteHelpText(Environment.NewLine);
            // WriteHelpHeader(@$"Example:");
            // WriteHelpText(@$"   craftsman add:message C:\fullpath\mymessageinfo.yaml");
            // WriteHelpText(@$"   craftsman add:message C:\fullpath\mymessageinfo.yml");
            // WriteHelpText(@$"   craftsman add:message C:\fullpath\mymessageinfo.json");
            // WriteHelpText(Environment.NewLine);
        }

        public static void Run(string filePath, string solutionDirectory, IFileSystem fileSystem)
        {
            try
            {
                FileParsingHelper.RunInitialTemplateParsingGuards(filePath);
                var template = FileParsingHelper.GetTemplateFromFile<AuthServerTemplate>(filePath);

                // get solution dir
                Utilities.IsSolutionDirectoryGuard(solutionDirectory);
                AddAuthServer(solutionDirectory, fileSystem, template);

                WriteHelpHeader($"{Environment.NewLine}Your messages have been successfully added. Keep up the good work!");
            }
            catch (Exception e)
            {
                if (e is SolutionNotFoundException
                    || e is DataValidationErrorException)
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

        public static void AddAuthServer(string solutionDirectory, IFileSystem fileSystem, AuthServerTemplate template)
        {
            SolutionBuilder.BuildAuthServerProject(solutionDirectory, template.Name, fileSystem);
            
            AuthServerLaunchSettingsBuilder.CreateLaunchSettings(solutionDirectory, template.Name, template.Port, fileSystem);
            StartupBuilder.CreateAuthServerStartup(solutionDirectory, template.Name, fileSystem);
            ProgramBuilder.CreateAuthServerProgram(solutionDirectory, template.Name, fileSystem);
            AuthServerConfigBuilder.CreateConfig(solutionDirectory, template, fileSystem);
            AppSettingsBuilder.CreateAuthServerAppSettings(solutionDirectory, template.Name, fileSystem);
            
            AuthServerPackageJsonBuilder.CreatePackageJson(solutionDirectory, template.Name, fileSystem);
            AuthServerTailwindConfigBuilder.CreateTailwindConfig(solutionDirectory, template.Name, fileSystem);
            AuthServerPostCssBuilder.CreatePostCss(solutionDirectory, template.Name, fileSystem);

            // controllers
            AuthServerAccountControllerBuilder.CreateAccountController(solutionDirectory, template.Name, fileSystem);
            AuthServerExternalControllerBuilder.CreateExternalController(solutionDirectory, template.Name, fileSystem);
            // AuthServerHomeControllerBuilder.CreateHomeController(projectDirectory, template.Name, fileSystem);
            
            // view models + models
            AuthServerAccountViewModelsBuilder.CreateViewModels(solutionDirectory, template.Name, fileSystem);
            AuthServerSharedViewModelsBuilder.CreateViewModels(solutionDirectory, template.Name, fileSystem);
            AuthServerExternalModelsBuilder.CreateModels(solutionDirectory, template.Name, fileSystem);
            AuthServerAccountModelsBuilder.CreateModels(solutionDirectory, template.Name, fileSystem);

            // views
            AuthServerAccountViewsBuilder.CreateLoginView(solutionDirectory, template.Name, fileSystem);
            AuthServerAccountViewsBuilder.CreateLogoutView(solutionDirectory, template.Name, fileSystem);
            AuthServerAccountViewsBuilder.CreateAccessDeniedView(solutionDirectory, template.Name, fileSystem);
            AuthServerSharedViewsBuilder.CreateLayoutView(solutionDirectory, template.Name, fileSystem);
            AuthServerSharedViewsBuilder.CreateStartView(solutionDirectory, template.Name, fileSystem);
            AuthServerSharedViewsBuilder.CreateViewImports(solutionDirectory, template.Name, fileSystem);
            
            // css files for TW
            AuthServerCssBuilder.CreateOutputCss(solutionDirectory, template.Name, fileSystem);
            AuthServerCssBuilder.CreateSiteCss(solutionDirectory, template.Name, fileSystem);
            
            // helpers
            AuthServerTestUsersBuilder.CreateTestModels(solutionDirectory, template.Name, fileSystem);
            AuthServerExtensionsBuilder.CreateExtensions(solutionDirectory, template.Name, fileSystem);
            SecurityHeadersAttributeBuilder.CreateAttribute(solutionDirectory, template.Name, fileSystem);
        }
    }
}