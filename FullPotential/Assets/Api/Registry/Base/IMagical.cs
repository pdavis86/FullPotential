using FullPotential.Api.Registry.Spells;

namespace FullPotential.Api.Registry.Base
{
    public interface IMagical
    {
        ISpellTargeting Targeting { get; }
        ISpellShape Shape { get; }
    }
}
