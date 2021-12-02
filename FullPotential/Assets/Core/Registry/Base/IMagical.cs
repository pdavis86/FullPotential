using FullPotential.Core.Spells.Shapes;
using FullPotential.Core.Spells.Targeting;

namespace FullPotential.Core.Registry.Base
{
    public interface IMagical
    {
        ISpellTargeting Targeting { get; }
        ISpellShape Shape { get; }
    }
}
