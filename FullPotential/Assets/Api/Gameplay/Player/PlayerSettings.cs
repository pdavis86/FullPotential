using Unity.Netcode;

namespace FullPotential.Api.Gameplay.Player
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
