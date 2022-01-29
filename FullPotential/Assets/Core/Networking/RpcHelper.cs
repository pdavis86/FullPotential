using Unity.Netcode;

namespace FullPotential.Core.Networking
{
    public static class RpcHelper
    {
        public static ClientRpcParams ForNearbyPlayers()
        {
            return new ClientRpcParams();
        }
    }
}
