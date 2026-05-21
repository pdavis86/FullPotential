using Cysharp.Threading.Tasks;

using FullPotential.Api.GameManagement.Models;

namespace FullPotential.Api.Data
{
    public interface IInstanceManagement
    {
        UniTask<ConnectionDetails> GetConnectionDetailsAsync();

        UniTask SaveConnectionDetailsAsync(ConnectionDetails connectionDetails);
    }
}
