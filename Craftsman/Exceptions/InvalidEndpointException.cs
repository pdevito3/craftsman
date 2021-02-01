namespace Craftsman.Exceptions
{
    using System;

    [Serializable]
    public class InvalidEndpointException : Exception
    {
        public InvalidEndpointException() : base($"The given endpoint was not recognized.")
        {

        }

        public InvalidEndpointException(string endpoint) : base($"The endpoint `{endpoint}` was not recognized.")
        {

        }
    }
}
