namespace Craftsman.Helpers
{
    using Craftsman.Models;
    using System;
    using System.Collections.Generic;

    public class TestBuildingHelpers
    {
        public static string GetUsingString(string dbContextName)
        {
            return $@"using (var context = new {dbContextName}(dbOptions))";
        }
    }
}
