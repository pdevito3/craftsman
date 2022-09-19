namespace Craftsman.Builders;

using System;
using System.IO;
using System.Text;
using Domain;
using Domain.Enums;
using Exceptions;
using Helpers;
using Services;

public class ValidatorBuilder
{
    private readonly ICraftsmanUtilities _utilities;

    public ValidatorBuilder(ICraftsmanUtilities utilities)
    {
        _utilities = utilities;
    }

    public void CreateValidators(string solutionDirectory, string srcDirectory, string projectBaseName, Entity entity)
    {
        BuildValidatorClass(solutionDirectory, srcDirectory, projectBaseName, entity, Validator.Manipulation);
        BuildValidatorClass(solutionDirectory, srcDirectory, projectBaseName, entity, Validator.Creation);
        BuildValidatorClass(solutionDirectory, srcDirectory, projectBaseName, entity, Validator.Update);
    }

    public void CreateRolePermissionValidators(string solutionDirectory, string srcDirectory, string projectBaseName, Entity entity)
    {
        BuildValidatorClass(solutionDirectory, srcDirectory, projectBaseName, entity, Validator.Creation);
        BuildValidatorClass(solutionDirectory, srcDirectory, projectBaseName, entity, Validator.Update);

        var manipulationClassPath = ClassPathHelper.ValidationClassPath(srcDirectory, $"{FileNames.ValidatorNameGenerator(entity.Name, Validator.Manipulation)}.cs", entity.Plural, projectBaseName);
        var manipulationFileText = GetRolePermissionManipulationValidatorFileText(solutionDirectory, srcDirectory, projectBaseName, manipulationClassPath.ClassNamespace, entity);
        _utilities.CreateFile(manipulationClassPath, manipulationFileText);
    }

    public void CreateUserValidators(string solutionDirectory, string srcDirectory, string projectBaseName, Entity entity)
    {
        BuildValidatorClass(solutionDirectory, srcDirectory, projectBaseName, entity, Validator.Creation);
        BuildValidatorClass(solutionDirectory, srcDirectory, projectBaseName, entity, Validator.Update);

        var manipulationClassPath = ClassPathHelper.ValidationClassPath(srcDirectory, $"{FileNames.ValidatorNameGenerator(entity.Name, Validator.Manipulation)}.cs", entity.Plural, projectBaseName);
        var manipulationFileText = GetUserManipulationValidatorFileText(solutionDirectory, srcDirectory, projectBaseName, manipulationClassPath.ClassNamespace, entity);
        _utilities.CreateFile(manipulationClassPath, manipulationFileText);
    }

    private static void BuildValidatorClass(string solutionDirectory, string srcDirectory, string projectBaseName, Entity entity, Validator validator)
    {
        var classPath = ClassPathHelper.ValidationClassPath(srcDirectory, $"{FileNames.ValidatorNameGenerator(entity.Name, validator)}.cs", entity.Plural, projectBaseName);

        if (!Directory.Exists(classPath.ClassDirectory))
            Directory.CreateDirectory(classPath.ClassDirectory);

        if (File.Exists(classPath.FullClassPath))
            throw new FileAlreadyExistsException(classPath.FullClassPath);

        using FileStream fs = File.Create(classPath.FullClassPath);
        var data = validator switch
        {
            Validator.Creation => GetCreationValidatorFileText(srcDirectory, classPath.ClassNamespace, entity, projectBaseName),
            Validator.Update => GetUpdateValidatorFileText(solutionDirectory, classPath.ClassNamespace, entity, projectBaseName),
            Validator.Manipulation => GetManipulationValidatorFileText(srcDirectory, classPath.ClassNamespace, entity, projectBaseName),
            _ => throw new Exception("Unrecognized validator exception.")
        };

        fs.Write(Encoding.UTF8.GetBytes(data));
    }

    public static string GetCreationValidatorFileText(string srcDirectory, string classNamespace, Entity entity, string projectBaseName)
    {
        var dtoClassPath = ClassPathHelper.DtoClassPath(srcDirectory, "", entity.Plural, projectBaseName);
        return @$"namespace {classNamespace};

using {dtoClassPath.ClassNamespace};
using FluentValidation;

public sealed class {FileNames.ValidatorNameGenerator(entity.Name, Validator.Creation)}: {FileNames.ValidatorNameGenerator(entity.Name, Validator.Manipulation)}<{FileNames.GetDtoName(entity.Name, Dto.Creation)}>
{{
    public {FileNames.ValidatorNameGenerator(entity.Name, Validator.Creation)}()
    {{
        // add fluent validation rules that should only be run on creation operations here
        //https://fluentvalidation.net/
    }}
}}";
    }

    public static string GetUpdateValidatorFileText(string srcDirectory, string classNamespace, Entity entity, string projectBaseName)
    {
        var dtoClassPath = ClassPathHelper.DtoClassPath(srcDirectory, "", entity.Plural, projectBaseName);
        return @$"namespace {classNamespace};

using {dtoClassPath.ClassNamespace};
using FluentValidation;

public sealed class {FileNames.ValidatorNameGenerator(entity.Name, Validator.Update)}: {FileNames.ValidatorNameGenerator(entity.Name, Validator.Manipulation)}<{FileNames.GetDtoName(entity.Name, Dto.Update)}>
{{
    public {FileNames.ValidatorNameGenerator(entity.Name, Validator.Update)}()
    {{
        // add fluent validation rules that should only be run on update operations here
        //https://fluentvalidation.net/
    }}
}}";
    }

    public static string GetManipulationValidatorFileText(string srcDirectory, string classNamespace, Entity entity, string projectBaseName)
    {
        var dtoClassPath = ClassPathHelper.DtoClassPath(srcDirectory, "", entity.Plural, projectBaseName);
        return @$"namespace {classNamespace};

using {dtoClassPath.ClassNamespace};
using FluentValidation;

public class {FileNames.ValidatorNameGenerator(entity.Name, Validator.Manipulation)}<T> : AbstractValidator<T> where T : {FileNames.GetDtoName(entity.Name, Dto.Manipulation)}
{{
    public {FileNames.ValidatorNameGenerator(entity.Name, Validator.Manipulation)}()
    {{
        // add fluent validation rules that should be shared between creation and update operations here
        //https://fluentvalidation.net/
    }}

    // want to do some kind of db check to see if something is unique? try something like this with the `MustAsync` prop
    // source: https://github.com/jasontaylordev/CleanArchitecture/blob/413fb3a68a0467359967789e347507d7e84c48d4/src/Application/TodoLists/Commands/CreateTodoList/CreateTodoListCommandValidator.cs
    // public async Task<bool> BeUniqueTitle(string title, CancellationToken cancellationToken)
    // {{
    //     return await _context.TodoLists
    //         .AllAsync(l => l.Title != title, cancellationToken);
    // }}
}}";
    }

    public static string GetRolePermissionManipulationValidatorFileText(string solutionDirectory, string srcDirectory, string projectBaseName, string classNamespace, Entity entity)
    {
        var dtoClassPath = ClassPathHelper.DtoClassPath(srcDirectory, "", entity.Plural, projectBaseName);
        var permissionsClassPath = ClassPathHelper.PolicyDomainClassPath(srcDirectory, "", projectBaseName);

        return @$"namespace {classNamespace};

using {dtoClassPath.ClassNamespace};
using {permissionsClassPath.ClassNamespace};
using FluentValidation;

public class {FileNames.ValidatorNameGenerator(entity.Name, Validator.Manipulation)}<T> : AbstractValidator<T> where T : {FileNames.GetDtoName(entity.Name, Dto.Manipulation)}
{{
    public {FileNames.ValidatorNameGenerator(entity.Name, Validator.Manipulation)}()
    {{
        RuleFor(rp => rp.Permission)
            .Must(BeAnExistingPermission)
            .WithMessage(""Please use a valid permission."");
    }}
    
    private static bool BeAnExistingPermission(string permission)
    {{
        return Permissions.List().Contains(permission, StringComparer.InvariantCultureIgnoreCase);
    }}
}}";
    }

    public static string GetUserManipulationValidatorFileText(string solutionDirectory, string srcDirectory, string projectBaseName, string classNamespace, Entity entity)
    {
        var dtoClassPath = ClassPathHelper.DtoClassPath(srcDirectory, "", entity.Plural, projectBaseName);
        var permissionsClassPath = ClassPathHelper.PolicyDomainClassPath(srcDirectory, "", projectBaseName);

        return @$"namespace {classNamespace};

using {dtoClassPath.ClassNamespace};
using {permissionsClassPath.ClassNamespace};
using FluentValidation;

public class {FileNames.ValidatorNameGenerator(entity.Name, Validator.Manipulation)}<T> : AbstractValidator<T> where T : {FileNames.GetDtoName(entity.Name, Dto.Manipulation)}
{{
    public {FileNames.ValidatorNameGenerator(entity.Name, Validator.Manipulation)}()
    {{
        RuleFor(u => u.Identifier)
            .NotEmpty()
            .WithMessage(""Please provide an identifier."");
    }}
}}";
    }
}
