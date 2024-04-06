using System;

namespace FullPotential.Api.Registry
{
    public interface IItemVisuals : IRegisterable, IHasPrefab
    {
        Guid ApplicableToTypeId { get; }
    }
}
