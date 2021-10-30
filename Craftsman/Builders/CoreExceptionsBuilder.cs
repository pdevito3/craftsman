namespace Craftsman.Builders
{
    using Craftsman.Exceptions;
    using Craftsman.Helpers;
    using System.IO;
    using System.Text;

    public class CoreExceptionsBuilder
    {
        public static void CreateExceptions(string solutionDirectory, string projectBaseName)
        {
            // ****this class path will have an invalid FullClassPath. just need the directory
            var classPath = ClassPathHelper.ExceptionsClassPath(solutionDirectory, "", projectBaseName);

            if (!Directory.Exists(classPath.ClassDirectory))
                Directory.CreateDirectory(classPath.ClassDirectory);

            CreateApiException(solutionDirectory, projectBaseName);
            CreateValidationException(solutionDirectory, projectBaseName);
            CreateConflictException(solutionDirectory, projectBaseName);
        }

        public static void CreateConflictException(string solutionDirectory, string projectBaseName)
        {
            var classPath = ClassPathHelper.ExceptionsClassPath(solutionDirectory, $"ConflictException.cs", projectBaseName);

            if (!Directory.Exists(classPath.ClassDirectory))
                Directory.CreateDirectory(classPath.ClassDirectory);

            if (File.Exists(classPath.FullClassPath))
                throw new FileAlreadyExistsException(classPath.FullClassPath);

            using FileStream fs = File.Create(classPath.FullClassPath);
            var data = "";
            data = GetConflictExceptionFileText(classPath.ClassNamespace);
            fs.Write(Encoding.UTF8.GetBytes(data));
        }

        public static void CreateApiException(string solutionDirectory, string projectBaseName)
        {
            var classPath = ClassPathHelper.ExceptionsClassPath(solutionDirectory, $"ApiException.cs", projectBaseName);

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
        }

        public static string GetApiExceptionFileText(string classNamespace)
        {
            return @$"namespace {classNamespace};

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
}}";
        }

        public static string GetConflictExceptionFileText(string classNamespace)
        {
            return @$"namespace {classNamespace};

using System;
using System.Globalization;

public class ConflictException : Exception
{{
    public ConflictException() : base() {{ }}

    public ConflictException(string message) : base(message) {{ }}

    public ConflictException(string message, params object[] args)
        : base(string.Format(CultureInfo.CurrentCulture, message, args))
    {{
    }}
}}";
        }

        public static void CreateValidationException(string solutionDirectory, string projectBaseName)
        {
            var classPath = ClassPathHelper.ExceptionsClassPath(solutionDirectory, $"ValidationException.cs", projectBaseName);

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
        }

        public static string GetValidationExceptionFileText(string classNamespace)
        {
            return @$"namespace {classNamespace};

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
}}";
        }
    }
}