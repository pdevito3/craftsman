namespace Craftsman.Tests.TemplateTests
{
    using System.Collections.Generic;
    using Fakes;
    using FluentAssertions;
    using Models;
    using Xunit;

    public class TemplateTests
    {
        [Fact]
        public void add_jwt_auth_is_true_when_feature_has_policy()
        {
            var feature = new Feature()
            {
                Policies = new List<Policy>()
                {
                    new()
                }
            };
            var entity = new Entity();
            entity.Features.Add(feature);
            var template = new AddEntityTemplate();
            template.Entities.Add(entity);

            template.AddJwtAuthentication.Should().BeTrue();
        }
        
        [Fact]
        public void add_jwt_auth_is_false_when_feature_has_policy()
        {
            var template = new AddEntityTemplate();

            template.AddJwtAuthentication.Should().BeFalse();
        }
    }
}
