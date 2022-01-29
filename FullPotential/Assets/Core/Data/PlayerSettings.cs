// ReSharper disable UnusedMember.Global
// ReSharper disable UnassignedField.Global

using Unity.Netcode;

namespace FullPotential.Core.Data
{
    [System.Serializable]
    public class PlayerSettings : INetworkSerializable
    {
        public string TextureUrl;

        public void NetworkSerialize<T>(BufferSerializer<T> serializer) where T : IReaderWriter
        {
            serializer.SerializeValue(ref TextureUrl);
        }
    }
}
