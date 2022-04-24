namespace Craftsman.Domain.Enums;

using Ardalis.SmartEnum;

public abstract class ExchangeTypeEnum : SmartEnum<ExchangeTypeEnum>
{
    public static readonly ExchangeTypeEnum Fanout = new FanoutType();
    public static readonly ExchangeTypeEnum Direct = new DirectType();
    public static readonly ExchangeTypeEnum Topic = new TopicType();

    protected ExchangeTypeEnum(string name, int value) : base(name, value)
    {
    }

    private class FanoutType : ExchangeTypeEnum
    {
        public FanoutType() : base(nameof(Fanout), 1) { }
    }

    private class DirectType : ExchangeTypeEnum
    {
        public DirectType() : base(nameof(Direct), 2) { }
    }

    private class TopicType : ExchangeTypeEnum
    {
        public TopicType() : base(nameof(Topic), 3) { }
    }
}
