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
    using System.Linq;
    using System.Threading.Tasks;
    using CommandLine.Text;
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

        public static void Run(string buildSolutionDirectory, IFileSystem fileSystem)
        {
            try
            {
                var promptResponse = RunPrompt();
                var domainProject = GetExampleDomain(promptResponse.name, promptResponse.type);

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

                AnsiConsole.MarkupLine($"{Environment.NewLine}[bold yellow1]Your example project is project is ready![/]");
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
        private static (ExampleType type, string name) RunPrompt()
        {
            AnsiConsole.WriteLine();
            AnsiConsole.Render(new Rule("[yellow]Create an Example Project[/]").RuleStyle("grey").Centered());

            var typeString = AskExampleType();
            var exampleType = ExampleType.FromName(typeString, ignoreCase: true);
            var projectName = AskExampleProjectName();

            return (exampleType, projectName);
        }

        private static string AskExampleType()
        {
            return AnsiConsole.Prompt(
                new SelectionPrompt<string>()
                    .Title("What [green]type of example[/] do you want to create?")
                    .PageSize(50)
                    .AddChoices(ExampleType.Basic.Name,
                        ExampleType.WithAuth.Name
                    )
            );
        }

        private static string AskExampleProjectName()
        {
            return AnsiConsole.Ask<string>("What would you like to name this project (e.g. [green]MyExampleProject[/])?");
        }
        
        private static DomainProject GetExampleDomain(string name, ExampleType exampleType)
        {
            if (exampleType == ExampleType.Basic)
                return GetBasicProject(name);
            if (exampleType == ExampleType.WithAuth)
                return GetWithAuthProject(name);

            throw new Exception("Example Type was not recognized.");
        }

        private static Entity BasicRecipeEntity()
        {
            return new Entity()
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
            };   
        }
        
        private static DomainProject GetBasicProject(string name)
        {
            var entity = BasicRecipeEntity();
            entity.Features.Add(new Feature() { Type = FeatureType.GetList.Name });
            entity.Features.Add(new Feature() { Type = FeatureType.GetRecord.Name });
            entity.Features.Add(new Feature() { Type = FeatureType.AddRecord.Name });
            entity.Features.Add(new Feature() { Type = FeatureType.UpdateRecord.Name });
            
            return new DomainProject()
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
                            BasicRecipeEntity()
                        }
                    }
                }
            };
        }

        private static DomainProject GetWithAuthProject(string name)
        {
            var template = GetBasicProject(name);
            var boundary = template.BoundedContexts.FirstOrDefault();
            boundary.Environments = new List<ApiEnvironment>()
            {
                new ApiEnvironment()
                {
                    EnvironmentName = "Development",
                    Authority = "https://localhost:5010",
                    Audience = "recipeManagementDev",
                    AuthorizationUrl = "https://localhost:5010/connect/authorize",
                    TokenUrl = "https://localhost:5010/connect/token",
                    ClientId = "service.client.dev"
                },
                new ApiEnvironment()
                {
                    EnvironmentName = "QA",
                    Authority = "https://qaauth.com",
                    Audience = "recipeManagementQa",
                    AuthorizationUrl = "https://qaauth.com/connect/authorize",
                    TokenUrl = "https://qaauth.com/connect/token",
                    ClientId = "service.client.qa",
                },
                new ApiEnvironment()
                {
                    EnvironmentName = "Production",
                    Authority = "https://prodauth.com",
                    Audience = "recipeManagement",
                    AuthorizationUrl = "https://prodauth.com/connect/authorize",
                    TokenUrl = "https://prodauth.com/connect/token",
                    ClientId = "service.client",
                }
            };

            var entity = BasicRecipeEntity();
            entity.Features.Add(new Feature()
            {
                Type = FeatureType.GetList.Name,
                Policies = new List<Policy>()
                {
                    new Policy() { Name = "CanReadRecipes", PolicyType = "scope", PolicyValue = "recipes.read" }
                }
            });
            entity.Features.Add(new Feature()
            {
                Type = FeatureType.GetRecord.Name,
                Policies = new List<Policy>()
                {
                    new Policy() { Name = "CanReadRecipes", PolicyType = "scope", PolicyValue = "recipes.read" }
                }
            });
            entity.Features.Add(new Feature()
            {
                Type = FeatureType.AddRecord.Name,
                Policies = new List<Policy>()
                {
                    new Policy() { Name = "CanAddRecipes", PolicyType = "scope", PolicyValue = "recipes.add" }
                }
            });
            entity.Features.Add(new Feature()
            {
                Type = FeatureType.UpdateRecord.Name,
                Policies = new List<Policy>()
                {
                    new Policy() { Name = "CanUpdateRecipes", PolicyType = "scope", PolicyValue = "recipes.update" }
                }
            });
            
            boundary.Entities.Clear();
            boundary.Entities.Add(entity);
            
            template.BoundedContexts.Clear();
            template.BoundedContexts.Add(boundary);

            return template;
        }
    }
}