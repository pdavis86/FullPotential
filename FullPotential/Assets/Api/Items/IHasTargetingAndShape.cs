using FullPotential.Api.Registry.Shapes;
using FullPotential.Api.Registry.Targeting;

// ReSharper disable UnusedMemberInSuper.Global

namespace FullPotential.Api.Items
{
    public interface IHasTargetingAndShape
    {
        ITargeting Targeting { get; set; }

        IShape Shape { get; set; }
    }
}
