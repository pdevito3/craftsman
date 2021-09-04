namespace Craftsman.Enums
{
    using System;
    using Ardalis.SmartEnum;
    
    public abstract class FeatureType : SmartEnum<FeatureType>
    {
        public static readonly FeatureType GetRecord = new GetRecordType();
        public static readonly FeatureType GetList = new GetListType();
        public static readonly FeatureType AddRecord = new AddRecordType();
        public static readonly FeatureType DeleteRecord = new DeleteRecordType();
        public static readonly FeatureType UpdateRecord = new UpdateRecordType();
        public static readonly FeatureType PatchRecord = new PatchRecordType();
        public static readonly FeatureType AdHoc = new AdHocType();
        public static readonly FeatureType AddListforFk = new AddListForFkType();

        protected FeatureType(string name, int value) : base(name, value)
        {
        }
        public abstract string FeatureName(string entityName, string featureName = null);
        public abstract string CommandName(string command, string entityName);
        
        private class GetRecordType : FeatureType
        {
            public GetRecordType() : base(nameof(GetRecord), 1) {}

            public override string FeatureName(string entityName, string featureName = null)
                => featureName.EscapeSpaces() ?? $"Get{entityName}";
            public override string CommandName(string command, string entityName)
                => command.EscapeSpaces() ?? $"Get{entityName}Query";
        }

        private class GetListType : FeatureType
        {
            public GetListType() : base(nameof(GetList), 2) {}

            public override string FeatureName(string entityName, string featureName = null) =>
                featureName.EscapeSpaces() ?? $"Get{entityName}List";
            public override string CommandName(string command, string entityName) =>
                command.EscapeSpaces() ?? $"Get{entityName}ListQuery";
        }

        private class AddRecordType : FeatureType
        {
            public AddRecordType() : base(nameof(AddRecord), 3) {}

            public override string FeatureName(string entityName, string featureName = null)
                => featureName.EscapeSpaces() ?? $"Add{entityName}";
            public override string CommandName(string command, string entityName)
                => command.EscapeSpaces() ?? $"Add{entityName}Command";
        }
        
        private class DeleteRecordType : FeatureType
        {
            public DeleteRecordType() : base(nameof(DeleteRecord), 4) {}

            public override string FeatureName(string entityName, string featureName = null)
                => featureName.EscapeSpaces() ?? $"Delete{entityName}";
            public override string CommandName(string command, string entityName)
                => command.EscapeSpaces() ?? $"Delete{entityName}Command";
        }
        
        
        private class UpdateRecordType : FeatureType
        {
            public UpdateRecordType() : base(nameof(UpdateRecord), 5) {}

            public override string FeatureName(string entityName, string featureName = null)
                => featureName.EscapeSpaces() ?? $"Update{entityName}";
            public override string CommandName(string command, string entityName)
                => command.EscapeSpaces() ?? $"Update{entityName}Command";
        }
        
        
        private class PatchRecordType : FeatureType
        {
            public PatchRecordType() : base(nameof(PatchRecord), 6) {}

            public override string FeatureName(string entityName, string featureName = null)
                => featureName.EscapeSpaces() ?? $"Patch{entityName}";
            public override string CommandName(string command, string entityName)
                => command.EscapeSpaces() ?? $"Patch{entityName}Command";
        }
        
        
        private class AdHocType : FeatureType
        {
            public AdHocType() : base(nameof(AdHoc), 7) {}

            public override string FeatureName(string entityName, string featureName = null)
                => featureName.EscapeSpaces() ?? throw new Exception("Ad Hoc Features require a name path.");
            public override string CommandName(string command, string entityName)
                => command.EscapeSpaces() ?? throw new Exception("Ad Hoc Features require a name path.");
        }

        private class AddListForFkType : FeatureType
        {
            public AddListForFkType() : base(nameof(AddListforFk), 8) {}

            public override string FeatureName(string entityName, string featureName = null)
                => featureName.EscapeSpaces() ?? $"Add{entityName}List";
            public override string CommandName(string command, string entityName)
                => command.EscapeSpaces() ?? $"Add{entityName}ListCommand";
        }
    }
}