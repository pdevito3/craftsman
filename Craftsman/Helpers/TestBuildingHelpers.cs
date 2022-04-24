namespace Craftsman.Helpers;

public class TestBuildingHelpers
{
    public static string GetUsingString(string dbContextName)
    {
        return $@"using (var context = new {dbContextName}(dbOptions))";
    }
}
