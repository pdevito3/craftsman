namespace Craftsman.Builders.Tests.FunctionalTests;

using Craftsman.Domain;
using Craftsman.Domain.Enums;
using Craftsman.Services;

public static class FunctionalTestServices
{
    public static string FakeParentTestHelpers(Entity entity, out string fakeParentIdRuleFor)
    {
        var fakeParent = "";
        fakeParentIdRuleFor = "";
        foreach (var entityProperty in entity.Properties)
        {
            if (entityProperty.IsForeignKey && !entityProperty.IsMany && entityProperty.IsPrimitiveType && entityProperty.IsPrimitiveType)
            {
                var baseVarName = entityProperty.ForeignEntityName != entity.Name
                    ? $"{entityProperty.ForeignEntityName}"
                    : $"{entityProperty.ForeignEntityName}Parent";
                var fakeParentClass = FileNames.FakerName(entityProperty.ForeignEntityName);
                var fakeParentCreationDto =
                    FileNames.FakerName(FileNames.GetDtoName(entityProperty.ForeignEntityName, Dto.Creation));
                fakeParent +=
                    @$"var fake{baseVarName}One = {fakeParentClass}.Generate(new {fakeParentCreationDto}().Generate());
        await InsertAsync(fake{baseVarName}One);{Environment.NewLine}{Environment.NewLine}        ";
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
                var fakeParentClass = FileNames.FakerName(entityProperty.ForeignEntityName);
                var fakeParentCreationDto =
                    FileNames.FakerName(FileNames.GetDtoName(entityProperty.ForeignEntityName, Dto.Creation));
                fakeParent +=
                    @$"var fake{baseVarName}One = {fakeParentClass}.Generate(new {fakeParentCreationDto}().Generate());
        var fake{baseVarName}Two = {fakeParentClass}.Generate(new {fakeParentCreationDto}().Generate());
        await InsertAsync(fake{baseVarName}One, fake{baseVarName}Two);{Environment.NewLine}{Environment.NewLine}        ";
                fakeParentIdRuleForOne +=
                    $"{Environment.NewLine}            .RuleFor({entity.Lambda} => {entity.Lambda}.{entityProperty.Name}, _ => fake{baseVarName}One.Id)";
                fakeParentIdRuleForTwo +=
                    $"{Environment.NewLine}            .RuleFor({entity.Lambda} => {entity.Lambda}.{entityProperty.Name}, _ => fake{baseVarName}Two.Id)";
            }
        }

        return fakeParent;
    }


    public static string FakeParentTestHelpersThreeCount(Entity entity, out string fakeParentIdRuleForOne, out string fakeParentIdRuleForTwo, out string fakeParentIdRuleForThree)
    {
        var fakeParent = "";
        fakeParentIdRuleForOne = "";
        fakeParentIdRuleForTwo = "";
        fakeParentIdRuleForThree = "";
        foreach (var entityProperty in entity.Properties)
        {
            if (entityProperty.IsForeignKey && !entityProperty.IsMany && entityProperty.IsPrimitiveType && entityProperty.IsPrimitiveType)
            {
                var baseVarName = entityProperty.ForeignEntityName != entity.Name
                    ? $"{entityProperty.ForeignEntityName}"
                    : $"{entityProperty.ForeignEntityName}Parent";
                var fakeParentClass = FileNames.FakerName(entityProperty.ForeignEntityName);
                var fakeParentCreationDto =
                    FileNames.FakerName(FileNames.GetDtoName(entityProperty.ForeignEntityName, Dto.Creation));
                fakeParent +=
                    @$"var fake{baseVarName}One = {fakeParentClass}.Generate(new {fakeParentCreationDto}().Generate());
        var fake{baseVarName}Two = {fakeParentClass}.Generate(new {fakeParentCreationDto}().Generate());
        var fake{baseVarName}Three = {fakeParentClass}.Generate(new {fakeParentCreationDto}().Generate());
        await InsertAsync(fake{baseVarName}One, fake{baseVarName}Two, fake{baseVarName}Three);{Environment.NewLine}{Environment.NewLine}        ";
                fakeParentIdRuleForOne +=
                    $"{Environment.NewLine}            .RuleFor({entity.Lambda} => {entity.Lambda}.{entityProperty.Name}, _ => fake{baseVarName}One.Id)";
                fakeParentIdRuleForTwo +=
                    $"{Environment.NewLine}            .RuleFor({entity.Lambda} => {entity.Lambda}.{entityProperty.Name}, _ => fake{baseVarName}Two.Id)";
                fakeParentIdRuleForThree +=
                    $"{Environment.NewLine}            .RuleFor({entity.Lambda} => {entity.Lambda}.{entityProperty.Name}, _ => fake{baseVarName}Three.Id)";
            }
        }

        return fakeParent;
    }
}