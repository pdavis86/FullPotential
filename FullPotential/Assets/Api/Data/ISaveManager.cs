using Cysharp.Threading.Tasks;

namespace FullPotential.Api.Data
{
    public interface ISaveManager
    {
        void AddToQueue(string username, ISaveable saveable);

        UniTask ProcessQueueForUsernameAsync(string username);

        UniTask ProcessQueueAsync();
    }
}
