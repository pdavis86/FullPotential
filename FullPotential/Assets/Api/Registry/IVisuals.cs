using System;

namespace FullPotential.Api.Registry
{
    public interface IVisuals : IRegisterable, IHasPrefab
    {
        Guid ApplicableToTypeId { get; }
    }
}
