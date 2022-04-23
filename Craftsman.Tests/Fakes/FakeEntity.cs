namespace Craftsman.Tests.Fakes
{
    using AutoBogus;
    using Domain;

    public class FakeEntity : AutoFaker<Entity>
    {
        public FakeEntity()
        {
        }
    }
}
