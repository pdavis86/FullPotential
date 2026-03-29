using System;
using System.Collections.Generic;

using FullPotential.Api.Data;
using FullPotential.Api.Obsolete;

namespace FullPotential.Core.Persistence.Https
{
    public class PlayerManagement : HttpsPersistenceBase, IPlayerManagement
    {
        public PlayerManagement(ISettingsRepository settingsRepository)
            : base(settingsRepository)
        {
        }

        public PlayerData Load(string username, bool reduced)
        {
            throw new NotImplementedException();
        }

        public void QueueAsapSave(string username)
        {
            throw new NotImplementedException();
        }

        public void SaveBatchPlayerData(Dictionary<ulong, string> clientIdToUsername, bool allData)
        {
            throw new NotImplementedException();
        }

        public void SavePlayerData(PlayerData playerData)
        {
            throw new NotImplementedException();
        }
    }
}
