namespace Craftsman.Exceptions
{
    using System;

    [Serializable]
    class EntityNotFoundException : Exception, ICraftsmanException
    {
        public EntityNotFoundException() : base($"Invalid file type. You need to use a json or yml file.")
        {

        }

        public EntityNotFoundException(string entity) : base($"The entity `{entity}` was not recognized.")
        {

        }
    }
}
