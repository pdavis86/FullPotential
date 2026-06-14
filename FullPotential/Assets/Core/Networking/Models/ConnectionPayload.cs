using System;

namespace FullPotential.Core.Networking.Models
{
    [Serializable]
    public struct ConnectionPayload
    {
        public string UserId;

        public string Username;

        public string Token;

        public string CharacterId;

        public string GameVersion;
    }
}
