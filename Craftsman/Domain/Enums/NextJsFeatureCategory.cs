namespace Craftsman.Domain.Enums;

using Ardalis.SmartEnum;

public abstract class NextJsFeatureCategory : SmartEnum<NextJsFeatureCategory>
{
    public static readonly NextJsFeatureCategory Index = new IndexType();
    public static readonly NextJsFeatureCategory Api = new ApiType();
    public static readonly NextJsFeatureCategory Routes = new RoutesType();
    public static readonly NextJsFeatureCategory Types = new TypesType();

    protected NextJsFeatureCategory(string name, int value) : base(name, value)
    {
    }

    private class IndexType : NextJsFeatureCategory
    {
        public IndexType() : base(nameof(Index), 1) { }
    }

    private class ApiType : NextJsFeatureCategory
    {
        public ApiType() : base(nameof(Api), 2) { }
    }

    private class RoutesType : NextJsFeatureCategory
    {
        public RoutesType() : base(nameof(Routes), 3) { }
    }

    private class TypesType : NextJsFeatureCategory
    {
        public TypesType() : base(nameof(Types), 4) { }
    }
}
