namespace NewCraftsman.Exceptions
{
    using System;

    [Serializable]
    public class InvalidFeatureTypeException : Exception, ICraftsmanException
    {
        public InvalidFeatureTypeException() : base($"The given feature type was not recognized.")
        {

        }

        public InvalidFeatureTypeException(string type) : base($"The feature type `{type}` was not recognized.")
        {

        }
    }
}
