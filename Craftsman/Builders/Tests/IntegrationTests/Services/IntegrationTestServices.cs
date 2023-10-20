namespace Craftsman.Builders.Tests.IntegrationTests.Services;

using Craftsman.Services;
using Domain;
using Domain.Enums;

public static class IntegrationTestServices
{
    public static string GetRandomId(string idType)
    {
        if (idType.Equals("string", StringComparison.InvariantCultureIgnoreCase))
            return @$"""badKey""";

        if (idType.Equals("guid", StringComparison.InvariantCultureIgnoreCase))
            return @$"Guid.NewGuid()";

        return idType.Equals("int", StringComparison.InvariantCultureIgnoreCase) ? @$"84709321" : "";
    }
}