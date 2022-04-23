namespace Craftsman.Tests.BuilderTests
{
    using System.Collections.Generic;
    using Builders;
    using Domain;
    using FluentAssertions;
    using Xunit;

    public class EntityBuilderTests
    {
        [Fact]
        public void two_one_type_props_formatted_properly()
        {
            var orgProp = new EntityProperty()
            {
                Name = "OrganizationId",
                Type = "Guid",
                ForeignEntityName = "Organization",
                ForeignEntityPlural = "Organizations"
            };
            var roleProp = new EntityProperty()
            {
                Name = "RoleId",
                Type = "Guid",
                ForeignEntityName = "Role",
                ForeignEntityPlural = "Roles"
            };

            var file = EntityBuilder.EntityPropBuilder(new List<EntityProperty>() { orgProp, roleProp });
            var expected = $@"    [JsonIgnore]
    [IgnoreDataMember]
    [ForeignKey(""Organization"")]
    public Guid OrganizationId {{ get; set; }}
    public Organization Organization {{ get; set; }}

    [JsonIgnore]
    [IgnoreDataMember]
    [ForeignKey(""Role"")]
    public Guid RoleId {{ get; set; }}
    public Role Role {{ get; set; }}";

            file.Should().Be(expected);
        }
        
        [Fact]
        public void one_prop_and_normal_prop()
        {
            var orgProp = new EntityProperty()
            {
                Name = "test",
                Type = "string",
            };
            var roleProp = new EntityProperty()
            {
                Name = "RoleId",
                Type = "Guid",
                ForeignEntityName = "Role",
                ForeignEntityPlural = "Roles"
            };

            var file = EntityBuilder.EntityPropBuilder(new List<EntityProperty>() { orgProp, roleProp });
            var expected = $@"    public string Test {{ get; set; }}

    [JsonIgnore]
    [IgnoreDataMember]
    [ForeignKey(""Role"")]
    public Guid RoleId {{ get; set; }}
    public Role Role {{ get; set; }}";

            file.Should().Be(expected);
        }
    }
}
