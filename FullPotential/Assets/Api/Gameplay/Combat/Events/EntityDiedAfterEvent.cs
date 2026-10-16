using FullPotential.Api.EventManagement;
using FullPotential.Api.Gameplay.Behaviours;

using UnityEngine;

namespace FullPotential.Api.Gameplay.Combat.Events
{
    public readonly struct EntityDiedAfterEvent : IEvent
    {
        public LivingEntityBase LivingEntity { get; }

        public Vector3 Position { get; }

        public string LastDamageSourceName { get; }

        public string LastDamageItemName { get; }

        public EntityDiedAfterEvent(LivingEntityBase livingEntity, Vector3 position, string lastDamageSourceName, string lastDamageItemName)
        {
            LivingEntity = livingEntity;
            Position = position;
            LastDamageSourceName = lastDamageSourceName;
            LastDamageItemName = lastDamageItemName;
        }
    }
}
