namespace Craftsman.Domain.Enums;

using System;
using Ardalis.SmartEnum;
using Helpers;
using Humanizer;

public abstract class FeatureType : SmartEnum<FeatureType>
{
    public static readonly FeatureType GetRecord = new GetRecordType();
    public static readonly FeatureType GetList = new GetListType();
    public static readonly FeatureType AddRecord = new AddRecordType();
    public static readonly FeatureType DeleteRecord = new DeleteRecordType();
    public static readonly FeatureType UpdateRecord = new UpdateRecordType();
    // public static readonly FeatureType PatchRecord = new PatchRecordType();
    public static readonly FeatureType AdHoc = new AdHocType();
    public static readonly FeatureType AddListByFk = new AddListByFkType();
    public static readonly FeatureType GetAll = new GetAllType();
    public static readonly FeatureType Job = new JobType();

    protected FeatureType(string name, int value) : base(name, value)
    {
    }
    public abstract string FeatureName(string entityName, string entityPlural, string featureName = null);
    public abstract string BffApiName(string entityName, string entityPlural);
    public abstract string NextJsApiName(string entityName, string entityPlural);
    public abstract string DefaultPermission(string entityPlural, string featureName);

    private class GetRecordType : FeatureType
    {
        public GetRecordType() : base(nameof(GetRecord), 1) { }

        public override string FeatureName(string entityName, string entityPlural, string featureName = null)
            => featureName.EscapeSpaces() ?? $"Get{entityName}";
        public override string BffApiName(string entityName, string entityPlural)
            => $"get{entityName}";
        public override string NextJsApiName(string entityName, string entityPlural)
            => $"get{entityName}";
        public override string DefaultPermission(string entityPlural, string featureName)
            => $"CanRead{entityPlural}";
    }

    private class GetListType : FeatureType
    {
        public GetListType() : base(nameof(GetList), 2) { }

        public override string FeatureName(string entityName, string entityPlural, string featureName = null)
            => featureName.EscapeSpaces() ?? $"Get{entityName}List";
        public override string BffApiName(string entityName, string entityPlural)
            => $"get{entityPlural}List";
        public override string NextJsApiName(string entityName, string entityPlural)
            => $"get{entityName}List";
        public override string DefaultPermission(string entityPlural, string featureName)
            => $"CanRead{entityPlural}";
    }

    private class AddRecordType : FeatureType
    {
        public AddRecordType() : base(nameof(AddRecord), 3) { }

        public override string FeatureName(string entityName, string entityPlural, string featureName = null)
            => featureName.EscapeSpaces() ?? $"Add{entityName}";
        public override string BffApiName(string entityName, string entityPlural)
            => $"add{entityName}";
        public override string NextJsApiName(string entityName, string entityPlural)
            => $"add{entityName}";
        public override string DefaultPermission(string entityPlural, string featureName)
            => $"CanAdd{entityPlural}";
    }

    private class DeleteRecordType : FeatureType
    {
        public DeleteRecordType() : base(nameof(DeleteRecord), 4) { }

        public override string FeatureName(string entityName, string entityPlural, string featureName = null)
            => featureName.EscapeSpaces() ?? $"Delete{entityName}";
        public override string BffApiName(string entityName, string entityPlural)
            => $"delete{entityName}";
        public override string NextJsApiName(string entityName, string entityPlural)
            => $"delete{entityName}";
        public override string DefaultPermission(string entityPlural, string featureName)
            => $"CanDelete{entityPlural}";
    }


    private class UpdateRecordType : FeatureType
    {
        public UpdateRecordType() : base(nameof(UpdateRecord), 5) { }

        public override string FeatureName(string entityName, string entityPlural, string featureName = null)
            => featureName.EscapeSpaces() ?? $"Update{entityName}";
        public override string BffApiName(string entityName, string entityPlural)
            => $"update{entityName}";
        public override string NextJsApiName(string entityName, string entityPlural)
            => $"update{entityName}";
        public override string DefaultPermission(string entityPlural, string featureName)
            => $"CanUpdate{entityPlural}";
    }


    // private class PatchRecordType : FeatureType
    // {
    //     public PatchRecordType() : base(nameof(PatchRecord), 6) { }
    //
    //     public override string FeatureName(string entityName, string entityPlural, string featureName = null)
    //         => featureName.EscapeSpaces() ?? $"Patch{entityName}";
    //     public override string CommandName(string command, string entityName)
    //         => command.EscapeSpaces() ?? $"Patch{entityName}Command";
    //     public override string BffApiName(string entityName, string entityPlural)
    //         => throw new Exception("Patch Features need to be manually configured in a BFF.");
    // }


    private class AdHocType : FeatureType
    {
        public AdHocType() : base(nameof(AdHoc), 7) { }

        public override string FeatureName(string entityName, string entityPlural, string featureName = null)
            => featureName.EscapeSpaces() ?? throw new Exception("Ad Hoc Features require a name path.");
        public override string BffApiName(string entityName, string entityPlural)
            => throw new Exception("Ad Hoc Features need to be manually configured in a BFF.");
        public override string NextJsApiName(string entityName, string entityPlural)
            => throw new Exception("Ad Hoc Features need to be manually configured in a BFF.");
        public override string DefaultPermission(string entityPlural, string featureName)
            => $"CanPerformAdHocFeature";
    }

    private class AddListByFkType : FeatureType
    {
        public AddListByFkType() : base(nameof(AddListByFk), 8) { }

        public override string FeatureName(string entityName, string entityPlural, string featureName = null)
            => featureName.EscapeSpaces() ?? $"Add{entityName}List";
        public override string BffApiName(string entityName, string entityPlural)
            => throw new Exception("Add List Features need to be manually configured in a BFF.");
        public override string NextJsApiName(string entityName, string entityPlural)
            => throw new Exception("Add List Features need to be manually configured in a BFF.");
        public override string DefaultPermission(string entityPlural, string featureName)
            => $"CanAdd{entityPlural}";
    }

    private class GetAllType : FeatureType
    {
        public GetAllType() : base(nameof(GetAll), 9) { }

        public override string FeatureName(string entityName, string entityPlural, string featureName = null)
            => featureName.EscapeSpaces() ?? $"GetAll{entityPlural}";
        public override string BffApiName(string entityName, string entityPlural)
            => $"getAll{entityPlural}";
        public override string NextJsApiName(string entityName, string entityPlural)
            => $"getAll{entityPlural}";
        public override string DefaultPermission(string entityPlural, string featureName)
            => $"CanRead{entityPlural}";
    }


    private class JobType : FeatureType
    {
        public JobType() : base(nameof(Job), 10) { }

        public override string FeatureName(string entityName, string entityPlural, string featureName = null)
            => featureName.EscapeSpaces() ?? throw new Exception("Job features require a name path.");
        public override string BffApiName(string entityName, string entityPlural)
            => throw new Exception("Job features need to be manually configured in a BFF.");
        public override string NextJsApiName(string entityName, string entityPlural)
            => throw new Exception("Job features need to be manually configured in a BFF.");
        public override string DefaultPermission(string entityPlural, string featureName)
            => $"CanPerform{featureName.Humanize(LetterCasing.Title).Replace(" ", "")}";
    }
}
