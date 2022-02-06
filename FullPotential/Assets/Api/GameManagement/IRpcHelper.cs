using Unity.Netcode;

namespace FullPotential.Api.GameManagement
{
    public interface IRpcHelper
    {
        ClientRpcParams ForNearbyPlayers();

        ClientRpcParams ForNearbyPlayersExceptMe();
    }
}
