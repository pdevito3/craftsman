namespace Craftsman.Helpers
{
    using System;
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
    using System.Linq;
    using Builders.Endpoints;
    using Builders.Tests.Utilities;
    using static Helpers.ConsoleWriter;

    public class EntityScaffolding
    {
        public static void ScaffoldEntities(string srcDirectory,
            string testDirectory,
            string projectBaseName,
            List<Entity> entities,
            string dbContextName,
            bool addSwaggerComments,
            IFileSystem fileSystem)
        {
            foreach (var entity in entities)
            {
                // not worrying about DTOs, profiles, validators, fakers - they are all added by default
                EntityBuilder.CreateEntity(srcDirectory, entity, projectBaseName, fileSystem);
                DtoBuilder.CreateDtos(srcDirectory, entity, projectBaseName);
                ValidatorBuilder.CreateValidators(srcDirectory, projectBaseName, entity);
                ProfileBuilder.CreateProfile(srcDirectory, entity, projectBaseName);
                ControllerBuilder.CreateController(srcDirectory, entity.Name, entity.Plural, projectBaseName);
                ApiRouteModifier.AddRoutes(testDirectory, entity, projectBaseName); // api routes always added to testing by default. too much of a pain to scaffold
                
                // TODO refactor to factory?
                foreach (var feature in entity.Features)
                {
                    AddFeatureToProject(srcDirectory, testDirectory, projectBaseName, dbContextName, addSwaggerComments, feature.Policies, feature, entity);
                }

                // Shared Tests
                FakesBuilder.CreateFakes(testDirectory, projectBaseName, entity);
            }
        }

        public static void AddFeatureToProject(string srcDirectory, string testDirectory, string projectBaseName,
            string dbContextName, bool addSwaggerComments, List<Policy> policies, Feature feature, Entity entity)
        {
            if (feature.Type == FeatureType.AddRecord.Name)
            {
                CommandAddRecordBuilder.CreateCommand(srcDirectory, entity, dbContextName, projectBaseName);
                AddCommandTestBuilder.CreateTests(testDirectory, entity, projectBaseName);
                CreateEntityTestBuilder.CreateTests(testDirectory, entity, policies, projectBaseName);
                ControllerModifier.AddEndpoint(srcDirectory, FeatureType.AddRecord, entity, addSwaggerComments, policies,
                    projectBaseName);
            }

            if (feature.Type == FeatureType.GetRecord.Name)
            {
                ScaffoldGetRecord(srcDirectory, testDirectory, projectBaseName, dbContextName, addSwaggerComments, policies, entity);
            }

            if (feature.Type == FeatureType.GetList.Name)
            {
                QueryGetListBuilder.CreateQuery(srcDirectory, entity, dbContextName, projectBaseName);
                GetListQueryTestBuilder.CreateTests(testDirectory, entity, projectBaseName);
                GetEntityListTestBuilder.CreateTests(testDirectory, entity, policies, projectBaseName);
                ControllerModifier.AddEndpoint(srcDirectory, FeatureType.GetList, entity, addSwaggerComments, policies,
                    projectBaseName);
            }
            
            if (feature.Type == FeatureType.DeleteRecord.Name)
            {
                CommandDeleteRecordBuilder.CreateCommand(srcDirectory, entity, dbContextName, projectBaseName);
                DeleteCommandTestBuilder.CreateTests(testDirectory, entity, projectBaseName);
                DeleteEntityTestBuilder.CreateTests(testDirectory, entity, policies, projectBaseName);
                ControllerModifier.AddEndpoint(srcDirectory, FeatureType.DeleteRecord, entity, addSwaggerComments, policies,
                    projectBaseName);
            }
            
            if (feature.Type == FeatureType.UpdateRecord.Name)
            {
                CommandUpdateRecordBuilder.CreateCommand(srcDirectory, entity, dbContextName, projectBaseName);
                PutCommandTestBuilder.CreateTests(testDirectory, entity, projectBaseName);
                PutEntityTestBuilder.CreateTests(testDirectory, entity, policies, projectBaseName);
                ControllerModifier.AddEndpoint(srcDirectory, FeatureType.UpdateRecord, entity, addSwaggerComments, policies,
                    projectBaseName);
            }
            
            if (feature.Type == FeatureType.PatchRecord.Name)
            {
                CommandPatchRecordBuilder.CreateCommand(srcDirectory, entity, dbContextName, projectBaseName);
                PatchCommandTestBuilder.CreateTests(testDirectory, entity, projectBaseName);
                PatchEntityTestBuilder.CreateTests(testDirectory, entity, policies, projectBaseName);
                ControllerModifier.AddEndpoint(srcDirectory, FeatureType.PatchRecord, entity, addSwaggerComments, policies,
                    projectBaseName);
            }

            if (feature.Type == FeatureType.AdHoc.Name)
            {
                EmptyFeatureBuilder.CreateCommand(srcDirectory, dbContextName, projectBaseName, feature);
                // TODO ad hoc feature endpoint
                // TODO empty failing test to promote test writing?
            }
        }

        public static void ScaffoldGetRecord(string srcDirectory, string testDirectory, string projectBaseName,
            string dbContextName, bool addSwaggerComments, List<Policy> policies, Entity entity)
        {
            QueryGetRecordBuilder.CreateQuery(srcDirectory, entity, dbContextName, projectBaseName);
            GetRecordQueryTestBuilder.CreateTests(testDirectory, entity, projectBaseName);
            GetEntityRecordTestBuilder.CreateTests(testDirectory, entity, policies, projectBaseName);
            ControllerModifier.AddEndpoint(srcDirectory, FeatureType.GetRecord, entity, addSwaggerComments, policies,
                projectBaseName);
        }
    }
}
