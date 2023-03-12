namespace Craftsman.Builders.EntityModels;

using System;
using System.Collections.Generic;
using Domain;
using Domain.Enums;
using Helpers;
using Services;

public static class EntityModelFileTextGenerator
{
    public static string GetEntityModelText(IClassPath modelClassPath, Entity entity, EntityModel model)
    {
        var propString = string.Empty;
        propString += EntityModelPropBuilder(entity.Properties, model);

        return @$"namespace {modelClassPath.ClassNamespace};

public sealed class {model.GetClassName(entity.Name)}
{{
{propString}
}}
";
    }

    public static string EntityModelPropBuilder(List<EntityProperty> props, EntityModel model)
    {
        var propString = "";
        for (var eachProp = 0; eachProp < props.Count; eachProp++)
        {
            if (!props[eachProp].CanManipulate && (model == EntityModel.Creation || model == EntityModel.Update))
                continue;
            if (props[eachProp].IsForeignKey && props[eachProp].IsMany)
                continue;
            if (!props[eachProp].IsPrimitiveType)
                continue;
            var guidDefault = model == EntityModel.Creation && props[eachProp].Type.IsGuidPropertyType()
                ? " = Guid.NewGuid();"
                : "";

            string newLine = eachProp == props.Count - 1 ? "" : Environment.NewLine;
            propString += $@"    public {props[eachProp].Type} {props[eachProp].Name} {{ get; set; }}{guidDefault}{newLine}";
        }

        return propString;
    }
}
