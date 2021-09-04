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
        public abstract string FeatureName(string name = null);
        public abstract string CommandName(string command, string entityName);
        
        private class GetRecordType : FeatureType
        {
            public GetRecordType() : base(nameof(GetRecord), 1) {}

            public override string FeatureName(string name = null)
                => name.EscapeSpaces() ?? "GetRecord";
            public override string CommandName(string command, string entityName)
                => command.EscapeSpaces() ?? $"Get{entityName}";
        }

        private class GetListType : FeatureType
        {
            public GetListType() : base(nameof(GetList), 2) {}

            public override string FeatureName(string name = null) =>
                name.EscapeSpaces() ?? "GetList";
            public override string CommandName(string command, string entityName) =>
                command.EscapeSpaces() ?? $"Get{entityName}List";
        }

        private class AddRecordType : FeatureType
        {
            public AddRecordType() : base(nameof(AddRecord), 3) {}

            public override string FeatureName(string name = null)
                => name.EscapeSpaces() ?? "Create";
            public override string CommandName(string command, string entityName)
                => command.EscapeSpaces() ?? $"Add{entityName}";
        }
        
        private class DeleteRecordType : FeatureType
        {
            public DeleteRecordType() : base(nameof(DeleteRecord), 4) {}

            public override string FeatureName(string name = null)
                => name.EscapeSpaces() ?? "Delete";
            public override string CommandName(string command, string entityName)
                => command.EscapeSpaces() ?? $"Delete{entityName}";
        }
        
        
        private class UpdateRecordType : FeatureType
        {
            public UpdateRecordType() : base(nameof(UpdateRecord), 5) {}

            public override string FeatureName(string name = null)
                => name.EscapeSpaces() ?? "UpdateRecord";
            public override string CommandName(string command, string entityName)
                => command.EscapeSpaces() ?? $"Update{entityName}";
        }
        
        
        private class PatchRecordType : FeatureType
        {
            public PatchRecordType() : base(nameof(PatchRecord), 6) {}

            public override string FeatureName(string name = null)
                => name.EscapeSpaces() ?? "PatchRecord";
            public override string CommandName(string command, string entityName)
                => command.EscapeSpaces() ?? $"Patch{entityName}";
        }
        
        
        private class AdHocType : FeatureType
        {
            public AdHocType() : base(nameof(AdHoc), 7) {}

            public override string FeatureName(string name = null)
                => name.EscapeSpaces() ?? throw new Exception("Ad Hoc Features require a name path.");
            public override string CommandName(string command, string entityName)
                => command.EscapeSpaces() ?? throw new Exception("Ad Hoc Features require a name path.");
        }

        private class AddListForFkType : FeatureType
        {
            public AddListForFkType() : base(nameof(AddListforFk), 8) {}

            public override string FeatureName(string name = null)
                => name.EscapeSpaces() ?? $"CreateList"; //TODO bring in entity name here...
            public override string CommandName(string command, string entityName)
                => command.EscapeSpaces() ?? $"Add{entityName}List";
        }
    }
}