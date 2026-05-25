using System;

using Unity.Netcode;

namespace FullPotential.Api.Data.Models
{
    [Serializable]
    public class CharacterSettings : INetworkSerializable
    {
        public string TextureUrl;

        public void NetworkSerialize<T>(BufferSerializer<T> serializer) where T : IReaderWriter
        {
            serializer.SerializeValue(ref TextureUrl);
        }
    }
}
