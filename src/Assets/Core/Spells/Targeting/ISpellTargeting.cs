using Assets.ApiScripts.Registry;

namespace Assets.Core.Spells.Targeting
{
    public interface ISpellTargeting : IRegisterable
    {
        bool HasShape { get; }
    }
}
