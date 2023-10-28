using System.Linq;
using FullPotential.Api.GameManagement;
using UnityEngine;

// ReSharper disable ClassNeverInstantiated.Global
// ReSharper disable UnusedType.Global

namespace FullPotential.Core.SceneObjects
{
    public class SceneObjectsStartup : MonoBehaviour
    {
        // ReSharper disable once UnusedMember.Local
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