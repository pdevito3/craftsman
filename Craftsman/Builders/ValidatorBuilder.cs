namespace Craftsman.Builders
{
    using Craftsman.Enums;
    using Craftsman.Exceptions;
    using Craftsman.Helpers;
    using Craftsman.Models;
    using System;
    using System.Collections.Generic;
    using System.IO;
    using System.Linq;
    using System.Reflection.Emit;
    using System.Text;
    using static Helpers.ConsoleWriter;

    public class ValidatorBuilder
    {
        public static void CreateValidators(string solutionDirectory, Entity entity)
        {
            try
            {
                BuildValidatorClass(solutionDirectory, entity, Validator.Manipulation);
                BuildValidatorClass(solutionDirectory, entity, Validator.Creation);
                BuildValidatorClass(solutionDirectory, entity, Validator.Update);
            }
            catch (FileAlreadyExistsException e)
            {
                WriteError(e.Message);
                throw;
            }
            catch (Exception e)
            {
                WriteError($"An unhandled exception occured when running the API command.\nThe error details are: \n{e.Message}");
                throw;
            }
        }

        private static void BuildValidatorClass(string solutionDirectory, Entity entity, Validator validator)
        {
            var classPath = ClassPathHelper.ValidationClassPath(solutionDirectory, $"{Utilities.ValidatorNameGenerator(entity.Name, validator)}.cs",entity.Name);

            if (!Directory.Exists(classPath.ClassDirectory))
                Directory.CreateDirectory(classPath.ClassDirectory);

            if (File.Exists(classPath.FullClassPath))
                throw new FileAlreadyExistsException(classPath.FullClassPath);

            using (FileStream fs = File.Create(classPath.FullClassPath))
            {
                var data = "";
                if (validator == Validator.Creation)
                    data = GetCreationValidatorFileText(classPath.ClassNamespace, entity);
                else if (validator == Validator.Update)
                    data = GetUpdateValidatorFileText(classPath.ClassNamespace, entity);
                else if (validator == Validator.Manipulation)
                    data = GetManipulationValidatorFileText(classPath.ClassNamespace, entity);
                else
                    throw new Exception("Unrecognized validator exception."); // this shouldn't really be possible, so not adding a special validator, but putting here for good measure

                fs.Write(Encoding.UTF8.GetBytes(data));
            }

            GlobalSingleton.AddCreatedFile(classPath.FullClassPath.Replace($"{solutionDirectory}\\", ""));
        }

        public static string GetCreationValidatorFileText(string classNamespace, Entity entity)
        {
            return @$"namespace {classNamespace}
{{
    using Application.Dtos.{entity.Name};

    public class {Utilities.ValidatorNameGenerator(entity.Name, Validator.Creation)}: {Utilities.ValidatorNameGenerator(entity.Name, Validator.Manipulation)}<{Utilities.DtoNameGenerator(entity.Name, Dto.Creation)}>
    {{
        public {Utilities.ValidatorNameGenerator(entity.Name, Validator.Creation)}()
        {{
            // add fluent validation rules that should only be run on creation operations here
            //https://fluentvalidation.net/
        }}
    }}
}}";
        }

        public static string GetUpdateValidatorFileText(string classNamespace, Entity entity)
        {
            return @$"namespace {classNamespace}
{{
    using Application.Dtos.{entity.Name};

    public class {Utilities.ValidatorNameGenerator(entity.Name, Validator.Update)}: {Utilities.ValidatorNameGenerator(entity.Name, Validator.Manipulation)}<{Utilities.DtoNameGenerator(entity.Name, Dto.Update)}>
    {{
        public {Utilities.ValidatorNameGenerator(entity.Name, Validator.Update)}()
        {{
            // add fluent validation rules that should only be run on update operations here
            //https://fluentvalidation.net/
        }}
    }}
}}";
        }

        public static string GetManipulationValidatorFileText(string classNamespace, Entity entity)
        {
            return @$"namespace {classNamespace}
{{
    using Application.Dtos.{entity.Name};
    using FluentValidation;
    using System;

    public class {Utilities.ValidatorNameGenerator(entity.Name, Validator.Manipulation)}<T> : AbstractValidator<T> where T : {Utilities.DtoNameGenerator(entity.Name, Dto.Manipulation)}
    {{
        public {Utilities.ValidatorNameGenerator(entity.Name, Validator.Manipulation)}()
        {{
            // add fluent validation rules that should be shared between creation and update operations here
            //https://fluentvalidation.net/
        }}
    }}
}}";
        }
    }
}
