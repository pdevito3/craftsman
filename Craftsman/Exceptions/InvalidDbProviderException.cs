namespace Craftsman.Exceptions
{
    using System;

    [Serializable]
    public class InvalidDbProviderException : Exception
    {
        public InvalidDbProviderException() : base($"The given database provider was not recognized.")
        {

        }

        public InvalidDbProviderException(string dbProvider) : base($"The database provider `{dbProvider}` was not recognized.")
        {

        }
    }
}
