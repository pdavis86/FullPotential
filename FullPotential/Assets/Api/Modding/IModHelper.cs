using FullPotential.Api.GameManagement;

namespace FullPotential.Api.Modding
{
    public interface IModHelper
    {
        IGameManager GetGameManager();
    }
}