namespace Craftsman.Exceptions;

using System;
using System.Collections.Generic;
using FluentValidation.Results;

[Serializable]
internal class DataValidationErrorException : Exception
{
    public DataValidationErrorException(List<ValidationFailure> failures) : base(GetValidationMessageString(failures))
    {
    }

    private static string GetValidationMessageString(List<ValidationFailure> failures)
    {
        var failString = "";
        foreach (var failure in failures)
        {
            failString += $"{Environment.NewLine}{failure.PropertyName} failed validation with the following issue: {failure.ErrorMessage}{Environment.NewLine}";
        }

        return $"The folowing data validation errors occured:{Environment.NewLine}{failString}";
    }
}
