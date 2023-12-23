namespace Craftsman.Exceptions;

using System;

internal class SolutionNotFoundException : Exception, ICraftsmanException
{
    public SolutionNotFoundException() : base($"A solution file was not found in your current directory. Please make sure you are in the solution directory for your project.")
    {
    }

    public SolutionNotFoundException(string message) : base(message)
    {
    }
}
