using System.Threading.Tasks;

using FullPotential.Api.Data;
using FullPotential.Api.GameManagement.Enums;
using FullPotential.Api.GameManagement.JsonModels;

using UnityEngine;

namespace FullPotential.Core.Persistence.Local
{
    public class InstanceManagement : IInstanceManagement
    {
        public async Awaitable<ConnectionDetails> GetConnectionDetailsAsync()
        {
            await Task.Yield();

            return new ConnectionDetails
            {
                Address = "127.0.0.1",
                Port = 7180,
                Status = InstanceState.Available
            };
        }
    }
}
