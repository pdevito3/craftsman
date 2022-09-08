namespace Craftsman.Domain.Enums;

using System;
using Ardalis.SmartEnum;
using Helpers;

public abstract class ValueObjectEnum : SmartEnum<ValueObjectEnum>
{
    public static readonly ValueObjectEnum Address = new AddressType();
    public static readonly ValueObjectEnum Role = new RoleType();
    public static readonly ValueObjectEnum Percent = new PercentType();
    public static readonly ValueObjectEnum MonetaryAmount = new MonetaryAmountType();

    protected ValueObjectEnum(string name, int value) : base(name, value)
    {
    }
    public abstract string Plural();

    private class AddressType : ValueObjectEnum
    {
        public AddressType() : base(nameof(Address), 0) { }

        public override string Plural()
            => "Addresses";
    }

    private class PercentType : ValueObjectEnum
    {
        public PercentType() : base(nameof(Percent), 1) { }

        public override string Plural()
            => "Percentages";
    }

    private class MonetaryAmountType : ValueObjectEnum
    {
        public MonetaryAmountType() : base(nameof(MonetaryAmount), 2) { }

        public override string Plural()
            => "MonetaryAmounts";
    }

    private class RoleType : ValueObjectEnum
    {
        public RoleType() : base(nameof(Role), 3) { }

        public override string Plural()
            => "Roles";
    }
}
