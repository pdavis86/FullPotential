using System;

namespace Assets.ApiScripts.Crafting
{
    public interface IRegisterable
    {
        Guid TypeId { get; }
        string TypeName { get; }
    }
}
