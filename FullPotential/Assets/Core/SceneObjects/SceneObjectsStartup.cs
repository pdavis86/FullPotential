using System.Linq;

using FullPotential.Api.Unity.Constants;

using UnityEngine;

namespace FullPotential.Core.SceneObjects
{
    public class SceneObjectsStartup : MonoBehaviour
    {
        private void Start()
        {
            //NOTE: Should use IUnityHelperUtilities but the game has not initialized yet
            UnityEngine.SceneManagement.SceneManager.GetActiveScene()
                .GetRootGameObjects()
                .First(x => x.name == GameObjectNames.SceneCanvas)
                .SetActive(true);
        }

    }
}