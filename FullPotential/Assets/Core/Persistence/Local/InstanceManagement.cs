using System.Threading.Tasks;

using Cysharp.Threading.Tasks;

using FullPotential.Api.Data;
using FullPotential.Api.Data.Models;
using FullPotential.Api.GameManagement;

namespace FullPotential.Core.Persistence.Local
{
    public class InstanceManagement : IInstanceManagement
    {
        public async UniTask<ConnectionDetails> GetConnectionDetailsAsync()
        {
            await Task.Yield();

            return new ConnectionDetails
            {
                Address = "127.0.0.1",
                Port = 7180,
                Status = InstanceState.Available
            };
        }

        public async UniTask SaveConnectionDetailsAsync(ConnectionDetails connectionDetails)
        {
            // Do nothing
            await Task.Yield();
        }
    }
}
