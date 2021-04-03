using System.Linq;
using UnityEngine;

namespace Assets.Core.Helpers
{
    public static class UnityHelper
    {
        public static GameObject GetObjectAtRoot(string name)
        {
            return UnityEngine.SceneManagement.SceneManager.GetActiveScene().GetRootGameObjects().FirstOrDefault(x => x.name == name);
        }

    }
}
