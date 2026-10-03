using System.Collections.Generic;

namespace FullPotential.Api.Modding
{
    public interface IMod
    {
        IEnumerable<string> GetNetworkPrefabAddresses();
    }
}
