using FullPotential.Api.Ioc;
using FullPotential.Api.Unity;
using FullPotential.Api.Unity.Constants;

using UnityEngine;

namespace FullPotential.Core.SceneObjects
{
    public class SceneObjectsOffline : MonoBehaviour
    {
        private IUnityHelperUtilities _unityHelperUtilities;

        private void Awake()
        {
            _unityHelperUtilities = DependenciesContext.Dependencies.GetService<IUnityHelperUtilities>();
        }

        private void Start()
        {
            _unityHelperUtilities.GetObjectAtRoot(GameObjectNames.SceneCanvas).SetActive(true);
        }

    }
}
