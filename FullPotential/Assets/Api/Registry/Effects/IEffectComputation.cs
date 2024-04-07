using FullPotential.Api.Gameplay.Behaviours;
using FullPotential.Api.Gameplay.Combat;
using FullPotential.Api.Items.Base;

namespace FullPotential.Api.Registry.Effects
{
    /// <summary>
    /// Only the last registered IEffectComputation will be used for any given EffectTypeId
    /// This is used for immunity, resistance, and vulnerability
    /// </summary>
    public interface IEffectComputation : IRegisterable
    {
        string EffectTypeId { get; }

        bool CanBeCriticalHit { get; }

        CombatResult GetCombatResult(
            FighterBase sourceFighter,
            ItemForCombatBase itemUsed,
            FighterBase targetFighter);
    }
}
