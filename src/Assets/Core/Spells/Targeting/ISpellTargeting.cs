using FullPotential.Assets.Api.Registry;

namespace FullPotential.Assets.Core.Spells.Targeting
{
    public interface ISpellTargeting : IRegisterable
    {
        bool HasShape { get; }
    }
}
