using Cysharp.Threading.Tasks;

namespace FullPotential.Api.Data
{
    public interface ISaveManager
    {
        void AddToQueue(string characterId, ISaveable saveable);

        UniTask ProcessQueueForCharacterIdAsync(string characterId);

        UniTask ProcessQueueAsync();
    }
}
