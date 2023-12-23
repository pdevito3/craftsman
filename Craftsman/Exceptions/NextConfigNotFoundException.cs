namespace Craftsman.Exceptions;

using System;

internal class NextConfigNotFoundException : Exception, ICraftsmanException
{
    public NextConfigNotFoundException() : base($"A NextJS config was not found in your current directory. Please make sure you are in the root of your NextJs project.")
    {
    }
}
