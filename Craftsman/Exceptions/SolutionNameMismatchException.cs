namespace Craftsman.Exceptions;

using System;

internal class SolutionNameMismatchException : Exception, ICraftsmanException
{
    public SolutionNameMismatchException() : base($"The solution name in your template file does not match the name of the solution in this directory. Please enter a matching solution name or remove it all together to use the directory solution name.")
    {
    }
}
