namespace Craftsman.Models
{
    using Craftsman.Exceptions;
    using System;
    using System.Collections.Generic;

    public class Policy
    {
        private PolicyType _policyType;

        /// <summary>
        /// The name of the policy (e.g. CanReadRecipes).
        /// </summary>
        public string Name { get; set; }

        /// <summary>
        /// The type of policy (scope, role, claim).
        /// </summary>
        public string PolicyType
        {
            get => Enum.GetName(typeof(PolicyType), _policyType);
            set
            {
                if (!Enum.TryParse<PolicyType>(value, true, out var parsed))
                {
                    throw new InvalidDbProviderException(value);
                }
                _policyType = parsed;
            }
        }

        /// <summary>
        /// The value of the policy (e.g. recipes.read).
        /// </summary>
        public string PolicyValue { get; set; }

        /// <summary>
        /// The entity endpoint information (e.g. Recipe, GetList, GetRecord).
        /// </summary>
        public List<EntityEndpoint> EndpointEntities { get; set; }
    }
}
