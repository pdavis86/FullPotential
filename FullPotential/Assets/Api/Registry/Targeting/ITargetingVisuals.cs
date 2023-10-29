using System;

namespace FullPotential.Api.Registry.Targeting
{
    public interface ITargetingVisuals : IVisuals
    {
        Guid TargetingTypeId { get; }
    }
}
