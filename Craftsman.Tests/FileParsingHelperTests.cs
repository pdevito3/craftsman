﻿namespace Craftsman.Tests
{
    using Craftsman.Commands;
    using Craftsman.Helpers;
    using Craftsman.Models;
    using FluentAssertions;
    using System;
    using System.Collections.Generic;
    using System.Data;
    using Xunit;

    public class FileParsingHelperTests
    {
        [Fact]
        public void RunPrimaryKeyGuard_throws_MissingPrimaryKeyException_with_no_pk_flag()
        {
            var template = new ApiTemplate()
            {
                Entities = new List<Entity>()
                {
                    new Entity() {}
                }
            };

            Action act = () => FileParsingHelper.RunPrimaryKeyGuard(template.Entities);
            act.Should().Throw<MissingPrimaryKeyException>();            
        }
    }
}
