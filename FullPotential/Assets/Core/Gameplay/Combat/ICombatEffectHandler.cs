using FullPotential.Api.Registry.Effects;

namespace FullPotential.Core.Gameplay.Combat
{
    public interface ICombatEffectHandler
    {
        bool CanHandle(IEffectType effect);

        void Apply(CombatEffectContext context, IEffectType effect);
    }
}
