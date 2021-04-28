using Assets.Core.Spells.Shapes;
using Assets.Core.Spells.Targeting;

namespace Assets.Core.Registry.Base
{
    public interface IMagical
    {
        ISpellTargeting Targeting { get; }
        ISpellShape Shape { get; }
    }
}
