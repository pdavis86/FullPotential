using UnityEngine;

namespace FullPotential.Api.Gameplay.Events
{
    public readonly struct PlayerJoinedEvent : IEvent
    {
        public string Username { get; }

        public Vector3 Position { get; }

        public ulong OwnerClientId { get; }

        public PlayerJoinedEvent(string username, Vector3 position, ulong ownerClientId)
        {
            Username = username;
            Position = position;
            OwnerClientId = ownerClientId;
        }
    }
}
