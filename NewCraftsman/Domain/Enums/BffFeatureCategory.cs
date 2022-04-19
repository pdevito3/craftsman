namespace NewCraftsman.Domain.Enums
{
    using Ardalis.SmartEnum;

    public abstract class BffFeatureCategory : SmartEnum<BffFeatureCategory>
    {
        public static readonly BffFeatureCategory Index = new IndexType();
        public static readonly BffFeatureCategory Api = new ApiType();
        public static readonly BffFeatureCategory Routes = new RoutesType();
        public static readonly BffFeatureCategory Types = new TypesType();

        protected BffFeatureCategory(string name, int value) : base(name, value)
        {
        }
        
        private class IndexType : BffFeatureCategory
        {
            public IndexType() : base(nameof(Index), 1) {}
        }

        private class ApiType : BffFeatureCategory
        {
            public ApiType() : base(nameof(Api), 2) {}
        }

        private class RoutesType : BffFeatureCategory
        {
            public RoutesType() : base(nameof(Routes), 3) {}
        }

        private class TypesType : BffFeatureCategory
        {
            public TypesType() : base(nameof(Types), 4) {}
        }
    }
}