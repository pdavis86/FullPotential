using FullPotential.Api.Gameplay.Behaviours;
using FullPotential.Api.Items;

using UnityEngine;

namespace FullPotential.Core.Gameplay.Combat
{
    public class CombatEffectContext
    {
        public FighterBase SourceFighter { get; }

        public ICombatItem ItemUsed { get; }

        public GameObject Target { get; }

        public Vector3? Position { get; }

        public CombatEffectContext(FighterBase sourceFighter, ICombatItem itemUsed, GameObject target, Vector3? position)
        {
            SourceFighter = sourceFighter;
            ItemUsed = itemUsed;
            Target = target;
            Position = position;
        }
    }
}
