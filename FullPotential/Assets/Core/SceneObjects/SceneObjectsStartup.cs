using FullPotential.Api.GameManagement.Constants;
using FullPotential.Api.Unity.Helpers;
using UnityEngine;

// ReSharper disable ClassNeverInstantiated.Global
// ReSharper disable once UnusedType.Global

namespace FullPotential.Core.SceneObjects
{
    public class SceneObjectsStartup : MonoBehaviour
    {
        // ReSharper disable once UnusedMember.Local
        private void Start()
        {
            GameObjectHelper.GetObjectAtRoot(GameObjectNames.SceneCanvas).SetActive(true);
        }

    }
}