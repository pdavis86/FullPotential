using FullPotential.Api.GameManagement;
using Unity.Netcode;

namespace FullPotential.Core.Networking
{
    public class RpcHelper : IRpcHelper
    {
        public ClientRpcParams ForNearbyPlayers()
        {
            return new ClientRpcParams();
        }

        public ClientRpcParams ForNearbyPlayersExceptMe()
        {
            return new ClientRpcParams();
        }
    }
}
