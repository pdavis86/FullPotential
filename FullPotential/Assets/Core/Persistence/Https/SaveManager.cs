using System;

using Cysharp.Threading.Tasks;

using FullPotential.Api.Data;

namespace FullPotential.Core.Persistence.Https
{
    public class SaveManager : ISaveManager
    {
        public void AddToQueue(string username, ISaveable saveable)
        {
            throw new NotImplementedException();
        }

        public UniTask ProcessQueueForUsernameAsync(string username)
        {
            throw new NotImplementedException();
        }

        public UniTask ProcessQueueAsync()
        {
            throw new NotImplementedException();
        }
    }
}
