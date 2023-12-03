using System.Collections.Generic;
using FullPotential.Api.Data;

namespace FullPotential.Api.Persistence
{
    public interface IPersistenceService
    {
        void QueueAsapSave(string username);

        void SaveBatchPlayerData(Dictionary<ulong, string> clientIdToUsernameMapping, bool allData = false);

        void SavePlayerData(PlayerData playerData);
    }
}
