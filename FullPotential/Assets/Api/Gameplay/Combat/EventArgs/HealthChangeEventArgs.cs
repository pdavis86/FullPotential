using FullPotential.Api.Gameplay.Behaviours;
using FullPotential.Api.Gameplay.Events;
using FullPotential.Api.Items.Base;
using UnityEngine;

namespace FullPotential.Api.Gameplay.Combat.EventArgs
{
    /// <summary>
    /// HealthChangeEvent is a special case event
    /// Not only does it apply the health change (or allow you to cancel it)
    /// But it also manages the showing of damage/heal indicators
    /// </summary>
    public class HealthChangeEventArgs : IEventHandlerArgs
    {
        public bool IsDefaultHandlerCancelled { get; set; }

        public LivingEntityBase LivingEntity { get; }

        public FighterBase SourceFighter { get; }

        public ItemBase ItemUsed { get; }

        public Vector3? Position { get; }

        public int Change { get; set; }

        public bool IsCritical { get; set; }

        public HealthChangeEventArgs(
            LivingEntityBase livingEntity,
            int change,
            FighterBase sourceFighter,
            ItemBase itemUsed,
            Vector3? position,
            bool isCritical)
        {
            LivingEntity = livingEntity;
            Change = change;
            SourceFighter = sourceFighter;
            ItemUsed = itemUsed;
            Position = position;
            IsCritical = isCritical;
        }
    }
}