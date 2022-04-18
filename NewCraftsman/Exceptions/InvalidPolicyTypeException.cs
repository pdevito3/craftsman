namespace NewCraftsman.Exceptions
{
    using System;

    [Serializable]
    public class InvalidPolicyTypeException : Exception
    {
        public InvalidPolicyTypeException() : base($"The given endpoint was not recognized.")
        {

        }

        public InvalidPolicyTypeException(string policyType) : base($"The policy type `{policyType}` was not recognized.")
        {

        }
    }
}
