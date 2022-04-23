namespace Craftsman.Builders.Tests.IntegrationTests.Services;

using Craftsman.Services;
using Domain;
using Domain.Enums;

public class IntegrationTestServices
{
    public static string FakeParentTestHelpers(Entity entity, out string fakeParentIdRuleFor)
    {
        var fakeParent = "";
        fakeParentIdRuleFor = "";
        foreach (var entityProperty in entity.Properties)
        {
            if (entityProperty.IsForeignKey && !entityProperty.IsMany && entityProperty.IsPrimativeType)
            {
                var fakeParentClass = FileNames.FakerName(entityProperty.ForeignEntityName);
                var fakeParentCreationDto =
                    FileNames.FakerName(FileNames.GetDtoName(entityProperty.ForeignEntityName, Dto.Creation));
                fakeParent +=
                    @$"var fake{entityProperty.ForeignEntityName}One = {fakeParentClass}.Generate(new {fakeParentCreationDto}().Generate());
        await InsertAsync(fake{entityProperty.ForeignEntityName}One);{Environment.NewLine}{Environment.NewLine}        ";
                fakeParentIdRuleFor +=
                    $"{Environment.NewLine}            .RuleFor({entity.Lambda} => {entity.Lambda}.{entityProperty.Name}, _ => fake{entityProperty.ForeignEntityName}One.Id){Environment.NewLine}            ";
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
            if (entityProperty.IsForeignKey && !entityProperty.IsMany && entityProperty.IsPrimativeType)
            {
                var fakeParentClass = FileNames.FakerName(entityProperty.ForeignEntityName);
                var fakeParentCreationDto =
                    FileNames.FakerName(FileNames.GetDtoName(entityProperty.ForeignEntityName, Dto.Creation));
                fakeParent +=
                    @$"var fake{entityProperty.ForeignEntityName}One = {fakeParentClass}.Generate(new {fakeParentCreationDto}().Generate());
    var fake{entityProperty.ForeignEntityName}Two = {fakeParentClass}.Generate(new {fakeParentCreationDto}().Generate());
    await InsertAsync(fake{entityProperty.ForeignEntityName}One, fake{entityProperty.ForeignEntityName}Two);{Environment.NewLine}{Environment.NewLine}        ";
                fakeParentIdRuleForOne +=
                    $"{Environment.NewLine}            .RuleFor({entity.Lambda} => {entity.Lambda}.{entityProperty.Name}, _ => fake{entityProperty.ForeignEntityName}One.Id){Environment.NewLine}            ";
                fakeParentIdRuleForTwo +=
                    $"{Environment.NewLine}            .RuleFor({entity.Lambda} => {entity.Lambda}.{entityProperty.Name}, _ => fake{entityProperty.ForeignEntityName}Two.Id){Environment.NewLine}            ";
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
            if (entityProperty.IsForeignKey && !entityProperty.IsMany && entityProperty.IsPrimativeType)
            {
                var fakeParentClass = FileNames.FakerName(entityProperty.ForeignEntityName);
                var fakeParentCreationDto =
                    FileNames.FakerName(FileNames.GetDtoName(entityProperty.ForeignEntityName, Dto.Creation));
                fakeParent +=
                    @$"var fake{entityProperty.ForeignEntityName}One = {fakeParentClass}.Generate(new {fakeParentCreationDto}().Generate());
    var fake{entityProperty.ForeignEntityName}Two = {fakeParentClass}.Generate(new {fakeParentCreationDto}().Generate());
    var fake{entityProperty.ForeignEntityName}Three = {fakeParentClass}.Generate(new {fakeParentCreationDto}().Generate());
    await InsertAsync(fake{entityProperty.ForeignEntityName}One, fake{entityProperty.ForeignEntityName}Two, fake{entityProperty.ForeignEntityName}Three);{Environment.NewLine}{Environment.NewLine}        ";
                fakeParentIdRuleForOne +=
                    $"{Environment.NewLine}            .RuleFor({entity.Lambda} => {entity.Lambda}.{entityProperty.Name}, _ => fake{entityProperty.ForeignEntityName}One.Id){Environment.NewLine}            ";
                fakeParentIdRuleForTwo +=
                    $"{Environment.NewLine}            .RuleFor({entity.Lambda} => {entity.Lambda}.{entityProperty.Name}, _ => fake{entityProperty.ForeignEntityName}Two.Id){Environment.NewLine}            ";
                fakeParentIdRuleForThree +=
                    $"{Environment.NewLine}            .RuleFor({entity.Lambda} => {entity.Lambda}.{entityProperty.Name}, _ => fake{entityProperty.ForeignEntityName}Three.Id){Environment.NewLine}            ";
            }
        }

        return fakeParent;
    }
}