namespace Craftsman.Commands
{
    using Craftsman.Builders;
    using Craftsman.Enums;
    using Craftsman.Exceptions;
    using Craftsman.Helpers;
    using Craftsman.Models;
    using Spectre.Console;
    using System;
    using System.Collections.Generic;
    using System.IO;
    using System.IO.Abstractions;
    using System.Threading.Tasks;
    using static Helpers.ConsoleWriter;

    public static class NewExampleCommand
    {
        public static void Help()
        {
            WriteHelpHeader(@$"Description:");
            WriteHelpText(@$"   Scaffolds out an example project.{Environment.NewLine}");

            WriteHelpHeader(@$"Usage:");
            WriteHelpText(@$"   craftsman new:example [options] arguments{Environment.NewLine}");

            WriteHelpHeader(@$"Arguments:");
            WriteHelpText(@$"   -t, --type         The type of example you'd like to create.");
            WriteHelpText(@$"   -n, --name         The name of the project you're creating.");

            WriteHelpText(Environment.NewLine);
            WriteHelpHeader(@$"Options:");
            WriteHelpText(@$"   -h, --help          Display this help message.");

            WriteHelpText(Environment.NewLine);
            WriteHelpHeader(@$"Example:");
            WriteHelpText(@$"   craftsman new:example --type basic --name MyExampleProjectName");
            WriteHelpText(@$"   craftsman new:example --type withauth --name MyExampleProjectName");
            WriteHelpText(@$"   craftsman new:example --type withbus --name MyExampleProjectName{Environment.NewLine}");
        }

        public static void Run(string type, string name, string buildSolutionDirectory, IFileSystem fileSystem)
        {
            try
            {
                //ExampleType
                var domainProject = GetExampleDomain(name);

                var domainDirectory = $"{buildSolutionDirectory}{Path.DirectorySeparatorChar}{domainProject.DomainName}";
                fileSystem.Directory.CreateDirectory(domainDirectory);
                SolutionBuilder.BuildSolution(domainDirectory, domainProject.DomainName, fileSystem);
                foreach (var bc in domainProject.BoundedContexts)
                    ApiScaffolding.ScaffoldApi(domainDirectory, bc, fileSystem);

                //TODO add yaml example file
                
                // messages
                if (domainProject.Messages.Count > 0)
                    AddMessageCommand.AddMessages(domainDirectory, fileSystem, domainProject.Messages);

                // migrations
                Utilities.RunDbMigrations(domainProject.BoundedContexts, domainDirectory);

                //final
                ReadmeBuilder.CreateReadme(domainDirectory, domainProject.DomainName, fileSystem);

                if (domainProject.AddGit)
                    Utilities.GitSetup(domainDirectory);

                AnsiConsole.MarkupLine($"{Environment.NewLine}[bold yellow1]Your domain project is ready! Build something amazing. [/]");
                StarGithubRequest();
            }
            catch (Exception e)
            {
                if (e is FileAlreadyExistsException
                    || e is DirectoryAlreadyExistsException
                    || e is InvalidSolutionNameException
                    || e is FileNotFoundException
                    || e is InvalidDbProviderException
                    || e is InvalidFileTypeException
                    || e is DataValidationErrorException
                    || e is SolutiuonNameEntityMatchException)
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

        private static DomainProject GetExampleDomain(string name)
        {
            var basic = new DomainProject()
            {
                DomainName = name,
                BoundedContexts = new List<ApiTemplate>()
                {
                    new ApiTemplate()
                    {
                        ProjectName = "RecipeManagement",
                        DbContext = new TemplateDbContext()
                        {
                            ContextName = "RecipeManagementDbContext",
                            DatabaseName = "RecipeManagement",
                            Provider = "Postgres"
                        },
                        Entities = new List<Entity>()
                        {
                            new Entity()
                            {
                                Name = "Recipe",
                                Properties = new List<EntityProperty>()
                                {
                                    new()
                                    {
                                        Name = "Title",
                                        Type = "string",
                                        CanFilter = true,
                                    },
                                    new()
                                    {
                                        Name = "Directions",
                                        Type = "string",
                                        CanFilter = true,
                                    },
                                    new()
                                    {
                                        Name = "Favorite",
                                        Type = "bool?",
                                        CanFilter = true,
                                    },
                                    new()
                                    {
                                        Name = "Rating",
                                        Type = "int?",
                                        CanFilter = true,
                                        CanSort = true,
                                    },
                                }
                            }
                        }
                    }
                }
                
            };

            return basic;
        }
    }
}