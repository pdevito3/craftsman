namespace Craftsman.Builders.Tests.IntegrationTests.Services;

using Craftsman.Services;
using Domain;
using Domain.Enums;

public static class IntegrationTestServices
{
    public static string FakeParentTestHelpersForBuilders(Entity entity, out string fakeParentIdRuleFor)
    {
        var fakeParent = "";
        fakeParentIdRuleFor = "";
        foreach (var entityProperty in entity.Properties)
        {
            if (entityProperty.IsForeignKey && !entityProperty.IsMany && entityProperty.IsPrimitiveType)
            {
                var baseVarName = entityProperty.ForeignEntityName != entity.Name
                    ? $"{entityProperty.ForeignEntityName}"
                    : $"{entityProperty.ForeignEntityName}Parent";
                var fakeParentBuilder = FileNames.FakeBuilderName(entityProperty.ForeignEntityName);
                fakeParent +=
                    @$"var fake{baseVarName}One = new {fakeParentBuilder}().Build();
        await testingServiceScope.InsertAsync(fake{baseVarName}One);{Environment.NewLine}{Environment.NewLine}        ";
                fakeParentIdRuleFor +=
                    $"{Environment.NewLine}            .With{entityProperty.Name}(fake{baseVarName}One.Id)";
            }
        }

        return fakeParent;
    }
    
    public static string FakeParentTestHelpersForUpdateDto(Entity entity, out string fakeParentIdRuleFor)
    {
        var fakeParent = "";
        fakeParentIdRuleFor = "";
        foreach (var entityProperty in entity.Properties)
        {
            if (entityProperty.IsForeignKey && !entityProperty.IsMany && entityProperty.IsPrimitiveType)
            {
                var baseVarName = entityProperty.ForeignEntityName != entity.Name
                    ? $"{entityProperty.ForeignEntityName}"
                    : $"{entityProperty.ForeignEntityName}Parent";
                var fakeParentClass = FileNames.FakerName(entityProperty.ForeignEntityName);
                var fakeParentCreationDto =
                    FileNames.FakerName(FileNames.GetDtoName(entityProperty.ForeignEntityName, Dto.Creation));
                fakeParent +=
                    @$"var fake{baseVarName}One = {fakeParentClass}.Generate(new {fakeParentCreationDto}().Generate());
        await testingServiceScope.InsertAsync(fake{baseVarName}One);{Environment.NewLine}{Environment.NewLine}        ";
                fakeParentIdRuleFor +=
                    $"{Environment.NewLine}            .RuleFor({entity.Lambda} => {entity.Lambda}.{entityProperty.Name}, _ => fake{baseVarName}One.Id)";
            }
        }

        return fakeParent;
    }

    public static string GetRandomId(string idType)
    {
        if (idType.Equals("string", StringComparison.InvariantCultureIgnoreCase))
            return @$"""badKey""";

        if (idType.Equals("guid", StringComparison.InvariantCultureIgnoreCase))
            return @$"Guid.NewGuid()";

        return idType.Equals("int", StringComparison.InvariantCultureIgnoreCase) ? @$"84709321" : "";
    }


    public static string FakeParentTestHelpersTwoCount(Entity entity, out string fakeParentIdRuleForOne, out string fakeParentIdRuleForTwo)
    {
        var fakeParent = "";
        fakeParentIdRuleForOne = "";
        fakeParentIdRuleForTwo = "";
        foreach (var entityProperty in entity.Properties)
        {
            if (entityProperty.IsForeignKey && !entityProperty.IsMany && entityProperty.IsPrimitiveType && entityProperty.IsPrimitiveType)
            {
                var baseVarName = entityProperty.ForeignEntityName != entity.Name
                    ? $"{entityProperty.ForeignEntityName}"
                    : $"{entityProperty.ForeignEntityName}Parent";
                var fakeParentBuilder = FileNames.FakeBuilderName(entityProperty.ForeignEntityName);
                fakeParent +=
                    @$"var fake{baseVarName}One = new {fakeParentBuilder}().Build();
        var fake{baseVarName}Two = new {fakeParentBuilder}().Build();
        await testingServiceScope.InsertAsync(fake{baseVarName}One, fake{baseVarName}Two);{Environment.NewLine}{Environment.NewLine}        ";
                fakeParentIdRuleForOne +=
                    $"{Environment.NewLine}            .With{entityProperty.Name}(fake{baseVarName}One.Id)";
                fakeParentIdRuleForTwo +=
                    $"{Environment.NewLine}            .With{entityProperty.Name}(fake{baseVarName}Two.Id)";
            }
        }

        return fakeParent;
    }
}