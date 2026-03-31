using System;
using System.Collections.Generic;

using FullPotential.Api.Data;
using FullPotential.Api.Obsolete;

using UnityEngine;

namespace FullPotential.Core.Persistence.Https
{
    public class PlayerManagement : HttpsPersistenceBase, IPlayerManagement
    {
        public PlayerManagement(ISettingsRepository settingsRepository)
            : base(settingsRepository)
        {
        }

        public Awaitable<PlayerData> LoadPlayerDataAsync(string username, bool reduced)
        {
            throw new NotImplementedException();
        }

        public Awaitable SavePlayerDataAsapAsync(string username)
        {
            throw new NotImplementedException();
        }

        public Awaitable SavePlayerDataBatchAsync(Dictionary<ulong, string> clientIdToUsernameMapping, bool allData)
        {
            throw new NotImplementedException();
        }

        public Awaitable SavePlayerDataImmediatelyAsync(PlayerData playerData)
        {
            throw new NotImplementedException();
        }
    }
}
