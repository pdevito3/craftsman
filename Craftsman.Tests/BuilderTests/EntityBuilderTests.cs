namespace Craftsman.Tests.FileTextTests
{
    using Craftsman.Builders;
    using Craftsman.Models;
    using Craftsman.Tests.Fakes;
    using FluentAssertions;
    using System;
    using System.Collections.Generic;
    using Xunit;
    using System.Linq;
    using AutoBogus;
    using System.IO.Abstractions.TestingHelpers;
    using System.IO;
    using Craftsman.Helpers;
    using Craftsman.Exceptions;

    public class EntityBuilderTests
    {
        [Fact]
        public void EntityBuilder_CreateEntity_createsEntityFile()
        {
            var solutionDirectory = @"c:\myrepo";
            var fileSystem = new MockFileSystem();
            fileSystem.AddDirectory(solutionDirectory);

            var entity = CannedGenerator.FakeBasicProduct();
            var expectedFilePath = ClassPathHelper.EntityClassPath(solutionDirectory, $"{entity.Name}.cs", "").FullClassPath;

            EntityBuilder.CreateEntity(solutionDirectory, entity, fileSystem);

            var exists = fileSystem.FileExists(expectedFilePath);

            exists.Should().BeTrue();
        }

        [Fact]
        public void EntityBuilder_CreateEntity_throws_error_if_file_exists()
        {
            var solutionDirectory = @"c:\myrepo";
            var fileSystem = new MockFileSystem();
            fileSystem.AddDirectory(solutionDirectory);

            var entity = CannedGenerator.FakeBasicProduct();
            var expectedFilePath = ClassPathHelper.EntityClassPath(solutionDirectory, $"{entity.Name}.cs", "").FullClassPath;
            fileSystem.AddFile(expectedFilePath, new MockFileData("content doesn't matter"));

            Assert.Throws<FileAlreadyExistsException>(() => EntityBuilder.CreateEntity(solutionDirectory, entity, fileSystem));
        }

        [Fact]
        public void EntityBuilder_TableAnnotationBuilder_creates_plural_name_when_no_schema_or_table_given()
        {
            var entity = new Entity();
            entity.Name = "SingularName";

            var annotation = EntityBuilder.TableAnnotationBuilder(entity);
            annotation.Should().Be(@"[Table(""SingularName"")]");            
        }

        [Fact]
        public void EntityBuilder_TableAnnotationBuilder_creates_table_name_table_given()
        {
            var entity = new Entity();
            entity.Name = "SinglularName";
            entity.TableName = "TableName";

            var annotation = EntityBuilder.TableAnnotationBuilder(entity);
            annotation.Should().Be(@"[Table(""TableName"")]");
        }

        [Fact]
        public void EntityBuilder_TableAnnotationBuilder_creates_table_name_with_schema()
        {
            var entity = new Entity();
            entity.Name = "SinglularName";
            entity.TableName = "TableName";
            entity.Schema = "Schema";

            var annotation = EntityBuilder.TableAnnotationBuilder(entity);
            annotation.Should().Be(@"[Table(""TableName"", Schema=""Schema"")]");
        }
    }
}
