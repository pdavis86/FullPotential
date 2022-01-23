using FullPotential.Api.Registry;

namespace FullPotential.Api.Spells
{
    public interface ISpellTargeting : IRegisterable
    {
        bool HasShape { get; }
    }
}
