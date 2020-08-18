namespace Craftsman.Tests.Fakes
{
    using AutoBogus;
    using Craftsman.Models;
    using System;
    using System.Collections.Generic;

    public class FakeEntity : AutoFaker<Entity>
    {
        public FakeEntity()
        {
        }
    }
}
