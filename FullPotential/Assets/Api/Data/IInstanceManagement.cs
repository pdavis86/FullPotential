using FullPotential.Api.GameManagement.JsonModels;

using UnityEngine;

namespace FullPotential.Api.Data
{
    public interface IInstanceManagement
    {
        Awaitable<ConnectionDetails> GetConnectionDetailsAsync();
    }
}
