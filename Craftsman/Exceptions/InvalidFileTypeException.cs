namespace Craftsman.Exceptions
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Text;
    using System.Threading.Tasks;

    [Serializable]
    class InvalidFileTypeException : Exception
    {
        public InvalidFileTypeException() : base($"Invalid file type. You need to use a json or yml file.")
        {

        }
    }
}
