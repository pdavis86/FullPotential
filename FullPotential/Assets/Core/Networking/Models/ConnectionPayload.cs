using System;

namespace FullPotential.Core.Networking.Models
{
    [Serializable]
    public struct ConnectionPayload
    {
        public string Username;
        
        public string Token;

        public string GameVersion;
    }
}
