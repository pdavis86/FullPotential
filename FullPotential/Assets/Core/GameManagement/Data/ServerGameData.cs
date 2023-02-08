using System.Collections.Generic;

namespace FullPotential.Core.GameManagement.Data
{
    public class ServerGameData
    {
        public Dictionary<ulong, string> ClientIdToUsername { get; set; }
    }
}
