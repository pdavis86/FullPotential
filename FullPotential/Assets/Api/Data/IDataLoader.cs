using Cysharp.Threading.Tasks;

using FullPotential.Api.Data.Models;
using FullPotential.Models;

namespace FullPotential.Api.Data
{
    public interface IDataLoader
    {
        UniTask<ConnectionDetails> GetConnectionDetailsAsync();

        UniTask<PlayerData> GetPlayerDataAsync(string username);

        UniTask<InventoryData> GetInventoryDataAsync(string username, bool reduced);
    }
}
