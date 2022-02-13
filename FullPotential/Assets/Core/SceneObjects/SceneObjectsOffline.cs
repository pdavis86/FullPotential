using FullPotential.Api.Unity.Helpers;
using FullPotential.Core.GameManagement.Constants;
using UnityEngine;

// ReSharper disable ClassNeverInstantiated.Global
// ReSharper disable once UnusedType.Global

namespace FullPotential.Core.SceneObjects
{
    public class SceneObjectsOffline : MonoBehaviour
    {
        // ReSharper disable once UnusedMember.Local
        private void Start()
        {
            GameObjectHelper.GetObjectAtRoot(GameObjectNames.SceneCanvas).SetActive(true);
        }

    }
}
