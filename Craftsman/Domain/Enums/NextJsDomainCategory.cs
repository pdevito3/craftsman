namespace Craftsman.Domain.Enums;

using Ardalis.SmartEnum;

public abstract class NextJsDomainCategory : SmartEnum<NextJsDomainCategory>
{
    public static readonly NextJsDomainCategory Index = new IndexType();
    public static readonly NextJsDomainCategory Api = new ApiType();
    public static readonly NextJsDomainCategory Routes = new RoutesType();
    public static readonly NextJsDomainCategory Types = new TypesType();
    public static readonly NextJsDomainCategory Features = new FeaturesType();

    protected NextJsDomainCategory(string name, int value) : base(name, value)
    {
    }

    private class IndexType : NextJsDomainCategory
    {
        public IndexType() : base(nameof(Index), 1) { }
    }

    private class ApiType : NextJsDomainCategory
    {
        public ApiType() : base(nameof(Api), 2) { }
    }

    private class RoutesType : NextJsDomainCategory
    {
        public RoutesType() : base(nameof(Routes), 3) { }
    }

    private class TypesType : NextJsDomainCategory
    {
        public TypesType() : base(nameof(Types), 4) { }
    }

    private class FeaturesType : NextJsDomainCategory
    {
        public FeaturesType() : base(nameof(Features), 5) { }
    }
}
