namespace Craftsman.Builders
{
    using Craftsman.Enums;
    using Craftsman.Exceptions;
    using Craftsman.Helpers;
    using Craftsman.Models;
    using System;
    using System.IO;
    using System.Text;

    public class ValidatorBuilder
    {
        public static void CreateValidators(string solutionDirectory, string projectBaseName, Entity entity)
        {
            BuildValidatorClass(solutionDirectory, projectBaseName, entity, Validator.Manipulation);

            // not building the creation and update ones anymore to KISS. Mainipulation can server as
            // shared validation. If there is shared validation required for just updates or just adds
            // then they can copy manipulation and make it themselves. I think this will be a rarity
            // enough that we can feel comfortable with this. even manip is possibly sharing prematurely

            //BuildValidatorClass(solutionDirectory, projectBaseName, entity, Validator.Creation);
            //BuildValidatorClass(solutionDirectory, projectBaseName, entity, Validator.Update);
        }

        private static void BuildValidatorClass(string solutionDirectory, string projectBaseName, Entity entity, Validator validator)
        {
            var classPath = ClassPathHelper.ValidationClassPath(solutionDirectory, $"{Utilities.ValidatorNameGenerator(entity.Name, validator)}.cs", entity.Plural, projectBaseName);

            if (!Directory.Exists(classPath.ClassDirectory))
                Directory.CreateDirectory(classPath.ClassDirectory);

            if (File.Exists(classPath.FullClassPath))
                throw new FileAlreadyExistsException(classPath.FullClassPath);

            using (FileStream fs = File.Create(classPath.FullClassPath))
            {
                var data = "";
                if (validator == Validator.Creation)
                    data = GetCreationValidatorFileText(solutionDirectory, projectBaseName, classPath.ClassNamespace, entity);
                else if (validator == Validator.Update)
                    data = GetUpdateValidatorFileText(solutionDirectory, projectBaseName, classPath.ClassNamespace, entity);
                else if (validator == Validator.Manipulation)
                    data = GetManipulationValidatorFileText(solutionDirectory, projectBaseName, classPath.ClassNamespace, entity);
                else
                    throw new Exception("Unrecognized validator exception."); // this shouldn't really be possible, so not adding a special validator, but putting here for good measure

                fs.Write(Encoding.UTF8.GetBytes(data));
            }
        }

        public static string GetCreationValidatorFileText(string solutionDirectory, string projectBaseName, string classNamespace, Entity entity)
        {
            var dtoClassPath = ClassPathHelper.DtoClassPath(solutionDirectory, "", entity.Name, projectBaseName);
            return @$"namespace {classNamespace};

using {dtoClassPath.ClassNamespace};
using FluentValidation;

public class {Utilities.ValidatorNameGenerator(entity.Name, Validator.Creation)}: {Utilities.ValidatorNameGenerator(entity.Name, Validator.Manipulation)}<{Utilities.GetDtoName(entity.Name, Dto.Creation)}>
{{
    public {Utilities.ValidatorNameGenerator(entity.Name, Validator.Creation)}()
    {{
        // add fluent validation rules that should only be run on creation operations here
        //https://fluentvalidation.net/
    }}
}}";
        }

        public static string GetUpdateValidatorFileText(string solutionDirectory, string projectBaseName, string classNamespace, Entity entity)
        {
            var dtoClassPath = ClassPathHelper.DtoClassPath(solutionDirectory, "", entity.Name, projectBaseName);
            return @$"namespace {classNamespace};

using {dtoClassPath.ClassNamespace};
using FluentValidation;

public class {Utilities.ValidatorNameGenerator(entity.Name, Validator.Update)}: {Utilities.ValidatorNameGenerator(entity.Name, Validator.Manipulation)}<{Utilities.GetDtoName(entity.Name, Dto.Update)}>
{{
    public {Utilities.ValidatorNameGenerator(entity.Name, Validator.Update)}()
    {{
        // add fluent validation rules that should only be run on update operations here
        //https://fluentvalidation.net/
    }}
}}";
        }

        public static string GetManipulationValidatorFileText(string solutionDirectory, string projectBaseName, string classNamespace, Entity entity)
        {
            var dtoClassPath = ClassPathHelper.DtoClassPath(solutionDirectory, "", entity.Name, projectBaseName);
            return @$"namespace {classNamespace};

using {dtoClassPath.ClassNamespace};
using FluentValidation;

public class {Utilities.ValidatorNameGenerator(entity.Name, Validator.Manipulation)}<T> : AbstractValidator<T> where T : {Utilities.GetDtoName(entity.Name, Dto.Manipulation)}
{{
    public {Utilities.ValidatorNameGenerator(entity.Name, Validator.Manipulation)}()
    {{
        // add fluent validation rules that should be shared between creation and update operations here
        //https://fluentvalidation.net/
    }}
}}";
        }
    }
}