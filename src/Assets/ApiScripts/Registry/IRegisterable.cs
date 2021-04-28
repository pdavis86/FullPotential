using System;

namespace Assets.ApiScripts.Registry
{
    public interface IRegisterable
    {
        Guid TypeId { get; }
        string TypeName { get; }
    }
}
