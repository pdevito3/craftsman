namespace Craftsman.Builders
{
    using Craftsman.Enums;
    using Craftsman.Exceptions;
    using Craftsman.Helpers;
    using Craftsman.Models;
    using System;
    using System.IO;
    using System.Text;
    using static Helpers.ConsoleWriter;

    public class CoreExceptionsBuilder
    {
        public static void CreateExceptions(string solutionDirectory, string projectBaseName)
        {
            try
            {
                // ****this class path will have an invalid FullClassPath. just need the directory
                var classPath = ClassPathHelper.CoreExceptionClassPath(solutionDirectory, "", projectBaseName);

                if (!Directory.Exists(classPath.ClassDirectory))
                    Directory.CreateDirectory(classPath.ClassDirectory);

                CreateApiException(solutionDirectory, projectBaseName);
                CreateValidationException(solutionDirectory, projectBaseName);
            }
            catch (FileAlreadyExistsException e)
            {
                WriteError(e.Message);
                throw;
            }
            catch (Exception e)
            {
                WriteError($"An unhandled exception occurred when running the API command.\nThe error details are: \n{e.Message}");
                throw;
            }
        }

        public static void CreateApiException(string solutionDirectory, string projectBaseName)
        {
            try
            {
                var classPath = ClassPathHelper.CoreExceptionClassPath(solutionDirectory, $"ApiException.cs", projectBaseName);

                if (!Directory.Exists(classPath.ClassDirectory))
                    Directory.CreateDirectory(classPath.ClassDirectory);

                if (File.Exists(classPath.FullClassPath))
                    throw new FileAlreadyExistsException(classPath.FullClassPath);

                using (FileStream fs = File.Create(classPath.FullClassPath))
                {
                    var data = "";
                    data = GetApiExceptionFileText(classPath.ClassNamespace);
                    fs.Write(Encoding.UTF8.GetBytes(data));
                }

                GlobalSingleton.AddCreatedFile(classPath.FullClassPath.Replace($"{solutionDirectory}{Path.DirectorySeparatorChar}", ""));
            }
            catch (FileAlreadyExistsException e)
            {
                WriteError(e.Message);
                throw;
            }
            catch (Exception e)
            {
                WriteError($"An unhandled exception occurred when running the API command.\nThe error details are: \n{e.Message}");
                throw;
            }
        }

        public static string GetApiExceptionFileText(string classNamespace)
        {
            return @$"namespace {classNamespace}
{{
    using System;
    using System.Globalization;

    public class ApiException : Exception
    {{
        public ApiException() : base() {{ }}

        public ApiException(string message) : base(message) {{ }}

        public ApiException(string message, params object[] args)
            : base(string.Format(CultureInfo.CurrentCulture, message, args))
        {{
        }}
    }}
}}";
        }

        public static void CreateValidationException(string solutionDirectory, string projectBaseName)
        {
            try
            {
                var classPath = ClassPathHelper.CoreExceptionClassPath(solutionDirectory, $"ValidationException.cs", projectBaseName);

                if (!Directory.Exists(classPath.ClassDirectory))
                    Directory.CreateDirectory(classPath.ClassDirectory);

                if (File.Exists(classPath.FullClassPath))
                    throw new FileAlreadyExistsException(classPath.FullClassPath);

                using (FileStream fs = File.Create(classPath.FullClassPath))
                {
                    var data = "";
                    data = GetValidationExceptionFileText(classPath.ClassNamespace);
                    fs.Write(Encoding.UTF8.GetBytes(data));
                }

                GlobalSingleton.AddCreatedFile(classPath.FullClassPath.Replace($"{solutionDirectory}{Path.DirectorySeparatorChar}", ""));
            }
            catch (FileAlreadyExistsException e)
            {
                WriteError(e.Message);
                throw;
            }
            catch (Exception e)
            {
                WriteError($"An unhandled exception occurred when running the API command.\nThe error details are: \n{e.Message}");
                throw;
            }
        }

        public static string GetValidationExceptionFileText(string classNamespace)
        {
            return @$"namespace {classNamespace}
{{
    using FluentValidation.Results;
    using System;
    using System.Collections.Generic;

    public class ValidationException : Exception
    {{
        public ValidationException() : base(""One or more validation failures have occurred."")
        {{
            Errors = new List<string>();
        }}
        public List<string> Errors {{ get; }}
        public ValidationException(IEnumerable<ValidationFailure> failures)
            : this()
        {{
            foreach (var failure in failures)
            {{
                Errors.Add(failure.ErrorMessage);
            }}
        }}
    }}
}}";
        }
    }
}
