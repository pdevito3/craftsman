namespace Craftsman.Domain.Enums;

using Ardalis.SmartEnum;

public abstract class EntityModel : SmartEnum<EntityModel>
{
    public static readonly EntityModel Creation = new CreationType();
    public static readonly EntityModel Update = new UpdateType();

    protected EntityModel(string name, int value) : base(name, value)
    {
    }
    public abstract string GetClassName(string entityName);

    private class CreationType : EntityModel
    {
        public CreationType() : base(nameof(Creation), 1) { }
        public override string GetClassName(string entityName) => $"{entityName}ForCreation";
    }

    private class UpdateType : EntityModel
    {
        public UpdateType() : base(nameof(Update), 2) { }
        public override string GetClassName(string entityName) => $"{entityName}ForUpdate";
    }
}
