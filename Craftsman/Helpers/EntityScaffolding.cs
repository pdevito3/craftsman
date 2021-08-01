namespace Craftsman.Helpers
{
    using Craftsman.Builders;
    using Craftsman.Builders.Dtos;
    using Craftsman.Builders.Features;
    using Craftsman.Builders.Tests.Fakes;
    using Craftsman.Builders.Tests.FunctionalTests;
    using Craftsman.Builders.Tests.IntegrationTests;
    using Craftsman.Enums;
    using Craftsman.Models;
    using System.Collections.Generic;
    using System.IO.Abstractions;
    using Builders.Endpoints;
    using static Helpers.ConsoleWriter;

    public class EntityScaffolding
    {
        public static void ScaffoldEntities(string srcDirectory,
            string testDirectory,
            string projectBaseName,
            List<Entity> entities,
            string dbContextName,
            bool addSwaggerComments,
            List<Policy> policies,
            IFileSystem fileSystem,
            Verbosity verbosity)
        {
            foreach (var entity in entities)
            {
                EntityBuilder.CreateEntity(srcDirectory, entity, projectBaseName, fileSystem);
                DtoBuilder.CreateDtos(srcDirectory, entity, projectBaseName);

                ValidatorBuilder.CreateValidators(srcDirectory, projectBaseName, entity);
                ProfileBuilder.CreateProfile(srcDirectory, entity, projectBaseName);

                QueryGetRecordBuilder.CreateQuery(srcDirectory, entity, dbContextName, projectBaseName);
                QueryGetListBuilder.CreateQuery(srcDirectory, entity, dbContextName, projectBaseName);
                CommandAddRecordBuilder.CreateCommand(srcDirectory, entity, dbContextName, projectBaseName);
                CommandDeleteRecordBuilder.CreateCommand(srcDirectory, entity, dbContextName, projectBaseName);
                CommandUpdateRecordBuilder.CreateCommand(srcDirectory, entity, dbContextName, projectBaseName);
                CommandPatchRecordBuilder.CreateCommand(srcDirectory, entity, dbContextName, projectBaseName);

                ControllerBuilder.CreateController(srcDirectory, entity, addSwaggerComments, policies, projectBaseName);

                // Shared Tests
                FakesBuilder.CreateFakes(testDirectory, projectBaseName, entity);

                // Integration Tests
                AddCommandTestBuilder.CreateTests(testDirectory, entity, projectBaseName);
                DeleteCommandTestBuilder.CreateTests(testDirectory, entity, projectBaseName);
                PatchCommandTestBuilder.CreateTests(testDirectory, entity, projectBaseName);
                GetRecordQueryTestBuilder.CreateTests(testDirectory, entity, projectBaseName);
                GetListQueryTestBuilder.CreateTests(testDirectory, entity, projectBaseName);
                PutCommandTestBuilder.CreateTests(testDirectory, entity, projectBaseName);

                // Functional Tests
                CreateEntityTestBuilder.CreateTests(testDirectory, entity, policies, projectBaseName);
                DeleteEntityTestBuilder.CreateTests(testDirectory, entity, policies, projectBaseName);
                GetEntityRecordTestBuilder.CreateTests(testDirectory, entity, policies, projectBaseName);
                GetEntityListTestBuilder.CreateTests(testDirectory, entity, policies, projectBaseName);
                PatchEntityTestBuilder.CreateTests(testDirectory, entity, policies, projectBaseName);
                PutEntityTestBuilder.CreateTests(testDirectory, entity, policies, projectBaseName);

                if (verbosity == Verbosity.More)
                    WriteHelpText($"{projectBaseName} '{entity.Name}' entity was scaffolded successfully.");
            }
        }
    }
}