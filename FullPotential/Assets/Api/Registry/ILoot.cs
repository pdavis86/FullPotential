using System;

namespace FullPotential.Api.Registry
{
    public interface ILoot : IRegisterable
    {
        Guid? ResourceTypeId { get; }
    }
}
