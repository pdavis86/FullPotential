using FullPotential.Api.Gameplay.Events;

using UnityEngine;

namespace FullPotential.Assets.Api.Gameplay.Combat.Events
{
    [RegisterEvent]
    public readonly struct EntityDiedAfterEvent : IEvent
    {
        public ulong OwnerClientId { get; }

        public string ObjectName { get; }

        public string EntityName { get; }

        public Vector3 Position { get; }

        public string LastDamageSourceName { get; }

        public string LastDamageItemName { get; }

        public EntityDiedAfterEvent(ulong ownerClientId, string objectName, string entityName, Vector3 position, string lastDamageSourceName, string lastDamageItemName)
        {
            OwnerClientId = ownerClientId;
            ObjectName = objectName;
            EntityName = entityName;
            Position = position;
            LastDamageSourceName = lastDamageSourceName;
            LastDamageItemName = lastDamageItemName;
        }
    }
}
