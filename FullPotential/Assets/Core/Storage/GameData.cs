using System.Collections.Generic;
using FullPotential.Core.Data;

// ReSharper disable ArrangeAccessorOwnerBody

namespace FullPotential.Core.Storage
{
    public class GameData
    {
        //todo: move this into the User Registry
        public Dictionary<string, PlayerData> PlayerData { get; set; }
    }
}
