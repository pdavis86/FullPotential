using FullPotential.Api.Gameplay.Events;

using UnityEngine;

namespace FullPotential.Api.Gameplay.Combat.Events
{
    [RegisterEvent]
    public readonly struct EntityDiedAfterEvent : IEvent
    {
        public string EntityName { get; }

        public Vector3 Position { get; }

        public string LastDamageSourceName { get; }

        public string LastDamageItemName { get; }

        public EntityDiedAfterEvent(string entityName, Vector3 position, string lastDamageSourceName, string lastDamageItemName)
        {
            EntityName = entityName;
            Position = position;
            LastDamageSourceName = lastDamageSourceName;
            LastDamageItemName = lastDamageItemName;
        }
    }
}
