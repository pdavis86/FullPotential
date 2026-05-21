using System;

using FullPotential.Api.Gameplay.Player;

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
