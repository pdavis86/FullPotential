using System.Collections.Generic;

using FullPotential.Api.Obsolete;

using UnityEngine;

namespace FullPotential.Api.Data
{
    public interface IPlayerManagement
    {
        Awaitable<PlayerData> LoadPlayerDataAsync(string username, bool reduced);

        Awaitable SavePlayerDataAsapAsync(string username);

        Awaitable SavePlayerDataImmediatelyAsync(PlayerData playerData);

        Awaitable SavePlayerDataBatchAsync(Dictionary<ulong, string> clientIdToUsernameMapping, bool allData);
    }
}
