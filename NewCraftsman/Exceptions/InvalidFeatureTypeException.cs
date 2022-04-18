namespace NewCraftsman.Exceptions
{
    using System;

    [Serializable]
    public class InvalidFeatureTypeException : Exception
    {
        public InvalidFeatureTypeException() : base($"The given feature type was not recognized.")
        {

        }

        public InvalidFeatureTypeException(string type) : base($"The feature type `{type}` was not recognized.")
        {

        }
    }
}
