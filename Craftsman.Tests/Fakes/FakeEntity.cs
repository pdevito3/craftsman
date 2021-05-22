namespace Craftsman.Tests.Fakes
{
    using AutoBogus;
    using Craftsman.Models;

    public class FakeEntity : AutoFaker<Entity>
    {
        public FakeEntity()
        {
        }
    }
}
