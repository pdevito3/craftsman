namespace Craftsman.Builders.Tests.Fakes;

using System;
using System.IO;
using Domain;
using Domain.Enums;
using Helpers;
using Services;

public class FakeEntityBuilderBuilder
{
    private readonly ICraftsmanUtilities _utilities;

    public FakeEntityBuilderBuilder(ICraftsmanUtilities utilities)
    {
        _utilities = utilities;
    }

    public void CreateFakeBuilder(string srcDirectory, string testDirectory, string projectBaseName, Entity entity)
    {
        // ****this class path will have an invalid FullClassPath. just need the directory
        var classPath = ClassPathHelper.TestFakesClassPath(testDirectory, $"", entity.Name, projectBaseName);

        if (!Directory.Exists(classPath.ClassDirectory))
            Directory.CreateDirectory(classPath.ClassDirectory);

        CreateFakeBuilderFile(srcDirectory, testDirectory, entity, projectBaseName);
    }

    private void CreateFakeBuilderFile(string srcDirectory, string testDirectory, Entity entity, string projectBaseName)
    {
        var classPath = ClassPathHelper.TestFakesClassPath(testDirectory, $"{FileNames.FakeBuilderName(entity.Name)}.cs", entity.Name, projectBaseName);
        var fileText = GetCreateFakeBuilderFileText(classPath.ClassNamespace, entity, srcDirectory, testDirectory, projectBaseName);
        _utilities.CreateFile(classPath, fileText);
    }

    private static string GetCreateFakeBuilderFileText(string classNamespace, Entity entity, string srcDirectory, string testDirectory, string projectBaseName)
    {
        var entitiesClassPath = ClassPathHelper.EntityClassPath(testDirectory, "", entity.Plural, projectBaseName);
        var modelClassPath = ClassPathHelper.EntityModelClassPath(srcDirectory, entity.Name, entity.Plural, null, projectBaseName);
        var creationModelName = EntityModel.Creation.GetClassName(entity.Name);
        var fakeCreationModelName = FileNames.FakerName(creationModelName);

        return @$"namespace {classNamespace};

using {entitiesClassPath.ClassNamespace};
using {modelClassPath.ClassNamespace};

public class {FileNames.FakeBuilderName(entity.Name)}
{{
    private {creationModelName} _creationData = new {fakeCreationModelName}().Generate();

    public {FileNames.FakeBuilderName(entity.Name)} WithModel({creationModelName} model)
    {{
        _creationData = model;
        return this;
    }}
    
    public {entity.Name} Build()
    {{
        var result = {entity.Name}.Create(_creationData);
        return result;
    }}
}}";
    }
}
