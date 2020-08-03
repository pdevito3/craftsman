namespace Craftsman.Exceptions
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Text;
    using System.Threading.Tasks;

    [Serializable]
    class InvalidSolutionNameException : Exception
    {
        public InvalidSolutionNameException() : base($"Invalid template file. Please enter a valid solution name.")
        {

        }
    }
}
