using FullPotential.Api.Registry;

namespace FullPotential.Core.Spells.Targeting
{
    public interface ISpellTargeting : IRegisterable
    {
        bool HasShape { get; }
    }
}
