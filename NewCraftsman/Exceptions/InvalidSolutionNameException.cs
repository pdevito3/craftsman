namespace NewCraftsman.Exceptions
{
    using System;

    [Serializable]
    class InvalidSolutionNameException : Exception, ICraftsmanException
    {
        public InvalidSolutionNameException() : base($"Invalid template file. Please enter a valid solution name.")
        {

        }
    }
}
