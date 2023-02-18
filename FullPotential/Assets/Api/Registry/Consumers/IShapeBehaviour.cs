using FullPotential.Api.Gameplay.Combat;
using FullPotential.Api.Items.Types;

namespace FullPotential.Api.Registry.Consumers
{
    public interface IShapeBehaviour
    {
        Consumer Consumer { set; }

        IFighter SourceFighter { set; }
    }
}
