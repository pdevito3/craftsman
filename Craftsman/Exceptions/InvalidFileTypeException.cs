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
        public InvalidFileTypeException()
        {

        }

        public InvalidFileTypeException(string fileType)
            : base($"Invalid file type of {fileType}.")
        {

        }

    }
}
