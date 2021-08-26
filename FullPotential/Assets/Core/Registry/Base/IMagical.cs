using FullPotential.Assets.Core.Spells.Shapes;
using FullPotential.Assets.Core.Spells.Targeting;

namespace FullPotential.Assets.Core.Registry.Base
{
    public interface IMagical
    {
        ISpellTargeting Targeting { get; }
        ISpellShape Shape { get; }
    }
}
