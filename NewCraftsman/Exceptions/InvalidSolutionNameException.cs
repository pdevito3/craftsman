namespace NewCraftsman.Exceptions
{
    using System;

    [Serializable]
    class InvalidSolutionNameException : Exception
    {
        public InvalidSolutionNameException() : base($"Invalid template file. Please enter a valid solution name.")
        {

        }
    }
}
