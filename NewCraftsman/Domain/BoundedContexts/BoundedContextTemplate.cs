namespace NewCraftsman.Domain.BoundedContexts
{
    using System.Collections.Generic;

    public class BoundedContextTemplate
    {
        public List<BoundedContext> BoundedContexts { get; set; } = new List<BoundedContext>();
    }
}
