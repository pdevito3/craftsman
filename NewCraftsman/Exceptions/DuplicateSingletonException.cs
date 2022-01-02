namespace Craftsman.Exceptions
{
    using System;

    [Serializable]
    public class DuplicateSingletonException : Exception
    {
        public DuplicateSingletonException() : base($"This singleton has been instantiated more than once.")
        {

        }    
    }
}
