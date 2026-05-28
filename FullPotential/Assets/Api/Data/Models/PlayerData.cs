using System;
using System.Collections.Generic;

namespace FullPotential.Api.Data.Models
{
    // todo: remove all this [Serializable] nonsense
    public class PlayerData
    {
        public string Username;
        public CharacterSettings Settings;
        public Dictionary<string, int> Resources;
    }
}
