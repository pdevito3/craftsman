﻿namespace Craftsman.Builders.Auth;

using Helpers;
using Services;

public class PermissionsBuilder
{
    private readonly ICraftsmanUtilities _utilities;

    public PermissionsBuilder(ICraftsmanUtilities utilities)
    {
        _utilities = utilities;
    }

    public void GetPermissions(string srcDirectory, string projectBaseName, bool hasAuth)
    {
        var classPath = ClassPathHelper.PolicyDomainClassPath(srcDirectory, "Permissions.cs", projectBaseName);
        var fileText = GetPermissionsText(classPath.ClassNamespace, hasAuth);
        _utilities.CreateFile(classPath, fileText);
    }

    private static string GetPermissionsText(string classNamespace, bool hasAuth)
    {
        var rolePermissions = "";
        if (hasAuth)
            rolePermissions += $@"
    public const string CanRemoveUserRoles = nameof(CanRemoveUserRoles);
    public const string CanAddUserRoles = nameof(CanAddUserRoles);
    public const string CanGetRoles = nameof(CanGetRoles);
    public const string CanGetPermissions = nameof(CanGetPermissions);";
        
        return @$"namespace {classNamespace};

using System.Reflection;

public static class Permissions
{{
    // Permissions marker - do not delete this comment
    public const string HangfireAccess = nameof(HangfireAccess);{rolePermissions}
    
    public static List<string> List()
    {{
        return typeof(Permissions)
            .GetFields(BindingFlags.Public | BindingFlags.Static | BindingFlags.FlattenHierarchy)
            .Where(fi => fi.IsLiteral && !fi.IsInitOnly && fi.FieldType == typeof(string))
            .Select(x => (string)x.GetRawConstantValue())
            .ToList();
    }}
}}";
    }
}
