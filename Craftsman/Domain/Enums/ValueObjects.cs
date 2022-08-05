namespace Craftsman.Domain.Enums;

using System;
using Ardalis.SmartEnum;
using Helpers;

public abstract class ValueObjects : SmartEnum<ValueObjects>
{
    public static readonly ValueObjects Address = new AddressType();
    public static readonly ValueObjects Percent = new PercentType();
    public static readonly ValueObjects MonetaryAmount = new MonetaryAmountType();

    protected ValueObjects(string name, int value) : base(name, value)
    {
    }
    public abstract string ClassNameWithoutExt();
    public abstract string Plural();

    private class AddressType : ValueObjects
    {
        public AddressType() : base(nameof(Address), 0) { }

        public override string ClassNameWithoutExt()
            => "Address";
        public override string Plural()
            => "Addresses";
    }

    private class PercentType : ValueObjects
    {
        public PercentType() : base(nameof(Percent), 1) { }

        public override string ClassNameWithoutExt()
            => "Percent";
        public override string Plural()
            => "Percents";
    }

    private class MonetaryAmountType : ValueObjects
    {
        public MonetaryAmountType() : base(nameof(MonetaryAmount), 2) { }

        public override string ClassNameWithoutExt()
            => "MonetaryAmount";
        public override string Plural()
            => "MonetaryAmounts";
    }
}
