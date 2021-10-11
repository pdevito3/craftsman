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
            var projectDirectory = $"{solutionDirectory}{Path.DirectorySeparatorChar}{template.Name}";
            SolutionBuilder.BuildAuthServerProject(solutionDirectory, template.Name, fileSystem);
            
            AuthServerLaunchSettingsBuilder.CreateLaunchSettings(projectDirectory, template.Name, template.Port, fileSystem);
            StartupBuilder.CreateAuthServerStartup(projectDirectory, template.Name, fileSystem);
            ProgramBuilder.CreateAuthServerProgram(projectDirectory, template.Name, fileSystem);
            AuthServerConfigBuilder.CreateConfig(projectDirectory, template, fileSystem);
            AppSettingsBuilder.CreateAuthServerAppSettings(projectDirectory, template.Name, fileSystem);
            
            AuthServerPackageJsonBuilder.CreatePackageJson(projectDirectory, template.Name, fileSystem);
            AuthServerTailwindConfigBuilder.CreateTailwindConfig(projectDirectory, template.Name, fileSystem);
            AuthServerPostCssBuilder.CreatePostCss(projectDirectory, template.Name, fileSystem);

            // controllers
            AuthServerAccountControllerBuilder.CreateAccountController(projectDirectory, template.Name, fileSystem);
            AuthServerExternalControllerBuilder.CreateExternalController(projectDirectory, template.Name, fileSystem);
            AuthServerHomeControllerBuilder.CreateHomeController(projectDirectory, template.Name, fileSystem);
            
            // view models + models
            AuthServerAccountViewModelsBuilder.CreateViewModels(projectDirectory, template.Name, fileSystem);
            AuthServerExternalModelsBuilder.CreateModels(projectDirectory, template.Name, fileSystem);
            AuthServerAccountModelsBuilder.CreateModels(projectDirectory, template.Name, fileSystem);
            
            // -- external VMs all in one file?
            
            // views
            AuthServerAccountViewsBuilder.CreateLoginView(projectDirectory, template.Name, fileSystem);
            AuthServerAccountViewsBuilder.CreateLogoutView(projectDirectory, template.Name, fileSystem);
            
            // helpers
            AuthServerTestUsersBuilder.CreateTestModels(projectDirectory, template.Name, fileSystem);
        }
    }
}