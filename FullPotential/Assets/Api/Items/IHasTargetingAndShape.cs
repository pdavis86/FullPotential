using FullPotential.Api.Registry.Consumers;

// ReSharper disable UnusedMemberInSuper.Global

namespace FullPotential.Api.Items
{
    public interface IHasTargetingAndShape
    {
        ITargeting Targeting { get; set; }

        IShape Shape { get; set; }
    }
}
