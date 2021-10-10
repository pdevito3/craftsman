namespace Craftsman.Enums
{
    using System;
    using Ardalis.SmartEnum;
    
    public abstract class GrantType : SmartEnum<GrantType>
    {
        public static readonly GrantType Code = new CodeType();
        public static readonly GrantType ClientCredentials = new ClientCredentialsType();

        protected GrantType(string name, int value) : base(name, value)
        {
        }
        
        private class CodeType : GrantType
        {
            public CodeType() : base(nameof(Code), 1) {}
        }

        private class ClientCredentialsType : GrantType
        {
            public ClientCredentialsType() : base(nameof(ClientCredentials), 2) {}
        }
    }
}