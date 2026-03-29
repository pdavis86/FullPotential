using System.Collections.Generic;

using FullPotential.Api.Obsolete;

namespace FullPotential.Api.Data
{
    public interface IPlayerManagement
    {
        PlayerData Load(string username, bool reduced);
        void QueueAsapSave(string username);
        void SaveBatchPlayerData(Dictionary<ulong, string> clientIdToUsername, bool allData);
        void SavePlayerData(PlayerData playerData);
    }
}
