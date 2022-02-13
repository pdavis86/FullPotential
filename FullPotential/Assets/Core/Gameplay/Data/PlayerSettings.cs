using Unity.Netcode;

namespace FullPotential.Core.Gameplay.Data
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
