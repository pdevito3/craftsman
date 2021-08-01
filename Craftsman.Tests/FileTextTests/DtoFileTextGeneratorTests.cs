namespace Craftsman.Tests.FileTextTests
{
    using Craftsman.Builders.Dtos;
    using Craftsman.Models;
    using Craftsman.Models.Interfaces;
    using FluentAssertions;
    using NSubstitute;
    using Xunit;

    public class DtoFileTextGeneratorTests
    {
        [Fact]
        public void GetDtoText_adds_pk_when_guid_for_creation_dto()
        {
            var classPath = Substitute.For<IClassPath>();
            classPath.ClassNamespace.Returns("Billing.Core.Dtos.Patient");

            var entity = new Entity();
            entity.Name = "Patient";
            entity.Properties.Add(new EntityProperty { Type = "guid", IsPrimaryKey = true, Name = "MyPrimaryKey" });
            var fileText = DtoFileTextGenerator.GetDtoText(classPath, entity, Enums.Dto.Creation);

            var expectedText = @$"namespace {classPath.ClassNamespace}
{{
    using System;
    using System.ComponentModel.DataAnnotations;
    using System.ComponentModel.DataAnnotations.Schema;

    public class {entity.Name}ForCreationDto : {entity.Name}ForManipulationDto
    {{
        public Guid MyPrimaryKey {{ get; set; }} = Guid.NewGuid();

        // add-on property marker - Do Not Delete This Comment
    }}
}}";

            fileText.Should().Be(expectedText);
        }

        [Fact]
        public void GetDtoText_has_no_props_when_pk_is_int_for_creation_dto()
        {
            var classPath = Substitute.For<IClassPath>();
            classPath.ClassNamespace.Returns("Billing.Core.Dtos.Patient");

            var entity = new Entity();
            entity.Name = "Patient";
            entity.Properties.Add(new EntityProperty { Type = "int", IsPrimaryKey = true, Name = "MyPrimaryKey" });
            var fileText = DtoFileTextGenerator.GetDtoText(classPath, entity, Enums.Dto.Creation);

            var expectedText = @$"namespace {classPath.ClassNamespace}
{{
    using System;
    using System.ComponentModel.DataAnnotations;
    using System.ComponentModel.DataAnnotations.Schema;

    public class {entity.Name}ForCreationDto : {entity.Name}ForManipulationDto
    {{
{""}

        // add-on property marker - Do Not Delete This Comment
    }}
}}";

            fileText.Should().Be(expectedText);
        }
    }
}