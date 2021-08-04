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
        public static readonly FeatureType AdHocRecord = new AdHocRecordType();

        protected FeatureType(string name, int value) : base(name, value)
        {
        }
        public abstract string Url(string url = null);
        public abstract string RouteTestingName(string url = null);

        private class GetRecordType : FeatureType
        {
            public GetRecordType() : base(nameof(GetRecord), 1) {}

            public override string RouteTestingName(string name = null)
                => name ?? "GetRecord";
            public override string Url(string url = null)
                => url.EscapeCurlyBraces() ?? $@"Base + ""/{{lowercaseEntityPluralName}}/"" + {{pkName}}";
        }

        private class GetListType : FeatureType
        {
            public GetListType() : base(nameof(GetList), 2) {}

            public override string RouteTestingName(string name = null) =>
                name.EscapeSpaces() ?? "GetList";
            public override string Url(string url = null) =>
                url.EscapeCurlyBraces() ?? $@"Base + ""/{{lowercaseEntityPluralName}}/""";
        }

        private class AddRecordType : FeatureType
        {
            public AddRecordType() : base(nameof(AddRecord), 3) {}

            public override string RouteTestingName(string name = null)
                => name ?? "Create";
            public override string Url(string url = null)
                => url.EscapeCurlyBraces() ?? $@"Base + ""/{{lowercaseEntityPluralName}}/""";
        }
        
        private class DeleteRecordType : FeatureType
        {
            public DeleteRecordType() : base(nameof(DeleteRecord), 4) {}

            public override string RouteTestingName(string name = null)
                => name ?? "Delete";
            public override string Url(string url = null)
                => url.EscapeCurlyBraces() ?? $@"Base + ""/{{lowercaseEntityPluralName}}/"" + {{pkName}}";
        }
        
        
        private class UpdateRecordType : FeatureType
        {
            public UpdateRecordType() : base(nameof(UpdateRecord), 5) {}

            public override string RouteTestingName(string name = null)
                => name ?? "UpdateRecord";
            public override string Url(string url = null)
                => url.EscapeCurlyBraces() ?? $@"Base + ""/{{lowercaseEntityPluralName}}/"" + {{pkName}}";
        }
        
        
        private class PatchRecordType : FeatureType
        {
            public PatchRecordType() : base(nameof(PatchRecord), 6) {}

            public override string RouteTestingName(string name = null)
                => name ?? "PatchRecord";
            public override string Url(string url = null)
                => url.EscapeCurlyBraces() ?? $@"Base + ""/{{lowercaseEntityPluralName}}/"" + {{pkName}}";
        }
        
        
        private class AdHocRecordType : FeatureType
        {
            public AdHocRecordType() : base(nameof(AdHocRecord), 7) {}

            public override string RouteTestingName(string name = null)
                => name.EscapeSpaces() ?? throw new Exception("Ad Hoc Features require a name path.");
            public override string Url(string url = null)
                => url.EscapeCurlyBraces() ?? throw new Exception("Ad Hoc Features require a url path.");
        }
    }
}