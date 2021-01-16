namespace Craftsman.Helpers
{
    using Craftsman.Models;
    using System;
    using System.Collections.Generic;

    public class TestBuildingHelpers
    {
        public static string GetUsingString(string authMethod, string dbContextName)
        {
            var usingString = $@"using (var context = new {dbContextName}(dbOptions))";
            if (authMethod == "JWT")
                usingString = $@"using (var context = new {dbContextName}(dbOptions, currentUserService, new DateTimeService()))";
            
            return usingString;
        }
        public static string GetUserServiceString(string authMethod)
        {
            var mockedUserString = "";
            if (authMethod == "JWT")
            {
                mockedUserString = $@"

            var currentUser = new Mock<ICurrentUserService>();
            currentUser.SetupGet(c => c.UserId).Returns(""testuser"");
            var currentUserService = currentUser.Object;";
            }

            return mockedUserString;
        }
    }
}
