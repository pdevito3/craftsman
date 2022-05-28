namespace Craftsman.Builders;

using Helpers;
using MediatR;
using Services;

public static class DomainEventBuilder
{
    public class DomainEventBuilderCommand : IRequest<bool>
    {
    }

    public class Handler : IRequestHandler<DomainEventBuilderCommand, bool>
    {
        private readonly ICraftsmanUtilities _utilities;
        private readonly IScaffoldingDirectoryStore _scaffoldingDirectoryStore;

        public Handler(ICraftsmanUtilities utilities,
            IScaffoldingDirectoryStore scaffoldingDirectoryStore)
        {
            _utilities = utilities;
            _scaffoldingDirectoryStore = scaffoldingDirectoryStore;
        }

        public Task<bool> Handle(DomainEventBuilderCommand request, CancellationToken cancellationToken)
        {
            var classPath = ClassPathHelper.EntityClassPath(_scaffoldingDirectoryStore.SrcDirectory, 
                "DomainEvent.cs", 
                "", 
                _scaffoldingDirectoryStore.ProjectBaseName);
            var fileText = GetFileText(classPath.ClassNamespace);
            _utilities.CreateFile(classPath, fileText);
            return Task.FromResult(true);
        }

        private static string GetFileText(string classNamespace)
        {
            return @$"namespace {classNamespace};

using System.Reflection;
using MediatR;

public abstract class DomainEvent : INotification, IEquatable<DomainEvent>
{{
    private List<PropertyInfo>? _properties;
    private List<FieldInfo>? _fields;

    public bool Equals(DomainEvent? obj)
    {{
        return Equals(obj as object);
    }}
    
    public override bool Equals(object? obj)
    {{
        if (obj == null || GetType() != obj.GetType())
            return false;
        return GetProperties().All(p => PropertiesAreEqual(obj, p)) && GetFields().All(f => FieldsAreEqual(obj, f));
    }}

    private bool PropertiesAreEqual(object obj, PropertyInfo p)
    {{
        return Equals(p.GetValue(this, null), p.GetValue(obj, null));
    }}

    private bool FieldsAreEqual(object obj, FieldInfo f)
    {{
        return Equals(f.GetValue(this), f.GetValue(obj));
    }}

    private IEnumerable<PropertyInfo> GetProperties()
    {{		
        return this._properties ??= GetType()
            .GetProperties(BindingFlags.Instance | BindingFlags.Public)
            .Where(p => p.GetCustomAttribute(typeof(IgnoreMemberAttribute)) == null)
            .ToList();
    }}

    private IEnumerable<FieldInfo> GetFields()
    {{
        return this._fields ??= GetType().GetFields(BindingFlags.Instance | BindingFlags.Public)
            .Where(p => p.GetCustomAttribute(typeof(IgnoreMemberAttribute)) == null)
            .ToList();
    }}

    public override int GetHashCode()
    {{
        unchecked //allow overflow
        {{
            int hash = 17;
            foreach (var prop in GetProperties())
            {{
                var value = prop.GetValue(this, null);
                hash = HashValue(hash, value);
            }}

            foreach (var field in GetFields())
            {{
                var value = field.GetValue(this);
                hash = HashValue(hash, value);
            }}

            return hash;
        }}
    }}

    private static int HashValue(int seed, object? value)
    {{
        var currentHash = value?.GetHashCode() ?? 0;
        return seed * 23 + currentHash;
    }}

    [AttributeUsage(AttributeTargets.Property | AttributeTargets.Field)]
    private class IgnoreMemberAttribute : Attribute
    {{
    }}
}}";
        }
    }
}
