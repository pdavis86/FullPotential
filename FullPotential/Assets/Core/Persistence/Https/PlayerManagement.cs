using System;

using Cysharp.Threading.Tasks;

using FullPotential.Api.Data;
using FullPotential.Api.Data.Models;

namespace FullPotential.Core.Persistence.Https
{
    public class PlayerManagement : HttpsPersistenceBase, IPlayerManagement
    {
        public PlayerManagement(ISettingsRepository settingsRepository)
            : base(settingsRepository)
        {
        }

        public UniTask<PlayerData> GetPlayerDataAsync(string username)
        {
            throw new NotImplementedException();
        }

        public UniTask SavePlayerDataAsync(PlayerData playerData)
        {
            throw new NotImplementedException();
        }

        public UniTask<InventoryData> GetInventoryDataAsync(string username, bool reduced)
        {
            throw new NotImplementedException();
        }

        public UniTask SaveInventoryChangesAsync(InventoryChanges inventoryChanges)
        {
            throw new NotImplementedException();
        }
    }
}
