using FullPotential.Api.GameManagement;
using FullPotential.Api.Modding;
using FullPotential.Core.GameManagement;

// ReSharper disable ClassNeverInstantiated.Global

namespace FullPotential.Core.Modding
{
    public class ModHelper : IModHelper
    {
        public IGameManager GetGameManager()
        {
            return GameManager.Instance;
        }
    }

}
