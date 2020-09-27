namespace Craftsman.Helpers
{
    using Craftsman.Models;
    using System;
    using System.Collections.Generic;

    public class TestBuildingHelpers
    {
        public static string GetUsingString(ApiTemplate template)
        {
            var usingString = $@"using (var context = new {template.DbContext.ContextName}(dbOptions))";
            if (template.AuthSetup.AuthMethod == "JWT")
                usingString = $@"using (var context = new {template.DbContext.ContextName}(dbOptions, currentUserService, new DateTimeService()))";
            
            return usingString;
        }
        public static string GetUserServiceString(ApiTemplate template)
        {
            var mockedUserString = "";
            if (template.AuthSetup.AuthMethod == "JWT")
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
