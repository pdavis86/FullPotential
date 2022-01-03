using FullPotential.Core.Helpers;
using UnityEngine;

// ReSharper disable UnusedMember.Global
// ReSharper disable UnusedMember.Local
// ReSharper disable ClassNeverInstantiated.Global
// ReSharper disable UnusedType.Global

namespace FullPotential.Core.Behaviours.SceneObjects
{
    public class SceneObjectsOffline : MonoBehaviour
    {
        private void Start()
        {
            GameObjectHelper.GetObjectAtRoot(Constants.GameObjectNames.SceneCanvas).SetActive(true);
        }

    }
}
