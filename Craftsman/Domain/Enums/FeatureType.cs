namespace Craftsman.Domain.Enums;

using System;
using Ardalis.SmartEnum;
using Helpers;

public abstract class FeatureType : SmartEnum<FeatureType>
{
    public static readonly FeatureType GetRecord = new GetRecordType();
    public static readonly FeatureType GetList = new GetListType();
    public static readonly FeatureType GetFormView = new GetFormViewType();
    public static readonly FeatureType GetListView = new GetListViewType();
    public static readonly FeatureType AddRecord = new AddRecordType();
    public static readonly FeatureType DeleteRecord = new DeleteRecordType();
    public static readonly FeatureType UpdateRecord = new UpdateRecordType();
    // public static readonly FeatureType PatchRecord = new PatchRecordType();
    public static readonly FeatureType AdHoc = new AdHocType();
    public static readonly FeatureType AddListByFk = new AddListByFkType();

    protected FeatureType(string name, int value) : base(name, value)
    {
    }
    public abstract string FeatureName(string entityName, string featureName = null);
    public abstract string CommandName();
    public abstract string BffApiName(string entityName);

    private class GetRecordType : FeatureType
    {
        public GetRecordType() : base(nameof(GetRecord), 1) { }

        public override string FeatureName(string entityName, string featureName = null)
            => featureName.EscapeSpaces() ?? $"Get{entityName}";
        public override string CommandName()
            => $"Query";
        public override string BffApiName(string entityName)
            => $"get{entityName}";
    }

    private class GetListType : FeatureType
    {
        public GetListType() : base(nameof(GetList), 2) { }

        public override string FeatureName(string entityName, string featureName = null)
            => featureName.EscapeSpaces() ?? $"Get{entityName}List";
        public override string CommandName()
            => $"Query";
        public override string BffApiName(string entityName)
            => $"get{entityName}List";
    }

    private class AddRecordType : FeatureType
    {
        public AddRecordType() : base(nameof(AddRecord), 3) { }

        public override string FeatureName(string entityName, string featureName = null)
            => featureName.EscapeSpaces() ?? $"Add{entityName}";
        public override string CommandName()
            => $"Command";
        public override string BffApiName(string entityName)
            => $"add{entityName}";
    }

    private class DeleteRecordType : FeatureType
    {
        public DeleteRecordType() : base(nameof(DeleteRecord), 4) { }

        public override string FeatureName(string entityName, string featureName = null)
            => featureName.EscapeSpaces() ?? $"Delete{entityName}";
        public override string CommandName()
            => $"Command";
        public override string BffApiName(string entityName)
            => $"delete{entityName}";
    }


    private class UpdateRecordType : FeatureType
    {
        public UpdateRecordType() : base(nameof(UpdateRecord), 5) { }

        public override string FeatureName(string entityName, string featureName = null)
            => featureName.EscapeSpaces() ?? $"Update{entityName}";
        public override string CommandName()
            => $"Command";
        public override string BffApiName(string entityName)
            => $"update{entityName}";
    }


    // private class PatchRecordType : FeatureType
    // {
    //     public PatchRecordType() : base(nameof(PatchRecord), 6) { }
    //
    //     public override string FeatureName(string entityName, string featureName = null)
    //         => featureName.EscapeSpaces() ?? $"Patch{entityName}";
    //     public override string CommandName(string command, string entityName)
    //         => ommand";
    //     public override string BffApiName(string entityName)
    //         => throw new Exception("Patch Features need to be manually configured in a BFF.");
    // }


    private class AdHocType : FeatureType
    {
        public AdHocType() : base(nameof(AdHoc), 7) { }

        public override string FeatureName(string entityName, string featureName = null)
            => featureName.EscapeSpaces() ?? throw new Exception("Ad Hoc Features require a name path.");
        public override string CommandName()
            => throw new Exception("Ad Hoc Features require a name path.");
        public override string BffApiName(string entityName)
            => throw new Exception("Ad Hoc Features need to be manually configured in a BFF.");
    }

    private class AddListByFkType : FeatureType
    {
        public AddListByFkType() : base(nameof(AddListByFk), 8) { }

        public override string FeatureName(string entityName, string featureName = null)
            => featureName.EscapeSpaces() ?? $"Add{entityName}List";
        public override string CommandName()
            => $"Command";
        public override string BffApiName(string entityName)
            => throw new Exception("Add List Features need to be manually configured in a BFF.");
    }

    private class GetFormViewType : FeatureType
    {
        public GetFormViewType() : base(nameof(GetFormView), 9) { }

        public override string FeatureName(string entityName, string featureName = null)
            => featureName.EscapeSpaces() ?? $"Get{entityName}FormView";
        public override string CommandName()
            => $"Query";
        public override string BffApiName(string entityName)
            => $"get{entityName}FormView";
    }

    private class GetListViewType : FeatureType
    {
        public GetListViewType() : base(nameof(GetListView), 10) { }

        public override string FeatureName(string entityName, string featureName = null)
            => featureName.EscapeSpaces() ?? $"Get{entityName}ListView";
        public override string CommandName()
            => $"Query";
        public override string BffApiName(string entityName)
            => $"get{entityName}ListView";
    }
}
