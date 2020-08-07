namespace Craftsman.Helpers
{
    using Craftsman.Models;
    using System;
    using System.Collections.Generic;

    public static class ClassPathHelper
    {
        public static ClassPath EntityClassPath(string solutionDirectory, string className)
        {
            return new ClassPath(solutionDirectory, "Domain\\Entities", className);
        }
    }
}
