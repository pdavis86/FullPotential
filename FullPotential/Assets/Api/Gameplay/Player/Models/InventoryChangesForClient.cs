using System;

using FullPotential.Api.Utilities.Extensions;

using Unity.Netcode;

namespace FullPotential.Api.Gameplay.Player.Models
{
    [Serializable]
    public class InventoryChangesForClient : INetworkSerializable
    {
        public string[] IdsToFetch;
        
        public string[] IdsToDelete;

        public string[] ChangedSlotsIds;

        public void NetworkSerialize<T>(BufferSerializer<T> serializer) where T : IReaderWriter
        {
            serializer.SerializeStringArray(ref IdsToFetch);
            serializer.SerializeStringArray(ref IdsToDelete);
            serializer.SerializeStringArray(ref ChangedSlotsIds);
        }
    }
}
