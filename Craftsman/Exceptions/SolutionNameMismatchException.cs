namespace Craftsman.Exceptions
{
    using System;
    using System.Runtime.Serialization;

    [Serializable]
    internal class SolutionNameMismatchException : Exception, ICraftsmanException
    {
        public SolutionNameMismatchException() : base($"The solution name in your template file does not match the name of the solution in this directory. Please enter a matching solution name or remove it all together to use the directory solution name.")
        {
        }

        public SolutionNameMismatchException(string message) : base(message)
        {
        }

        public SolutionNameMismatchException(string message, Exception innerException) : base(message, innerException)
        {
        }

        protected SolutionNameMismatchException(SerializationInfo info, StreamingContext context) : base(info, context)
        {
        }
    }
}