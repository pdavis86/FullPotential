using Assets.Core.Crafting.SpellShapes;
using Assets.Core.Crafting.SpellTargeting;

namespace Assets.Core.Crafting.Base
{
    public interface IMagical
    {
        ISpellTargeting Targeting { get; }
        ISpellShape Shape { get; }
    }
}
