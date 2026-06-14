using System.Collections.Generic;

using FullPotential.Core.Networking.Models;

namespace FullPotential.Core.GameManagement.Data
{
    public class ServerGameData
    {
        public Dictionary<ulong, ConnectionPayload> ClientIdToConnectionPayload { get; } = new Dictionary<ulong, ConnectionPayload>();
    }
}
