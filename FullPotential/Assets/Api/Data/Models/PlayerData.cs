using System;

namespace FullPotential.Api.Data.Models
{
    [Serializable]
    public class PlayerData
    {
        public string Username;
        public CharacterSettings Settings;
        public SerializableKeyValuePair<string, int>[] Resources;
    }
}
