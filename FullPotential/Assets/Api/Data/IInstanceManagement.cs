using Cysharp.Threading.Tasks;

using FullPotential.Api.Data.Models;

// ReSharper disable UnusedMember.Global
// ReSharper disable UnusedParameter.Global

namespace FullPotential.Api.Data
{
    public interface IInstanceManagement
    {
        UniTask<ConnectionDetails> GetConnectionDetailsAsync();

        UniTask SaveConnectionDetailsAsync(ConnectionDetails connectionDetails);
    }
}
