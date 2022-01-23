using FullPotential.Api.Spells;

namespace FullPotential.Core.Registry.Base
{
    public interface IMagical
    {
        ISpellTargeting Targeting { get; }
        ISpellShape Shape { get; }
    }
}
