using FullPotential.Api.GameManagement;
using FullPotential.Api.Ioc;
using FullPotential.Api.Unity.Services;
using UnityEngine;

// ReSharper disable ClassNeverInstantiated.Global
// ReSharper disable UnusedType.Global

namespace FullPotential.Core.SceneObjects
{
    public class SceneObjectsOffline : MonoBehaviour
    {
        private IUnityHelperUtilities _unityHelperUtilities;

        // ReSharper disable once UnusedMember.Local
        private void Awake()
        {
            _unityHelperUtilities = DependenciesContext.Dependencies.GetService<IUnityHelperUtilities>();
        }

        // ReSharper disable once UnusedMember.Local
        private void Start()
        {
            _unityHelperUtilities.GetObjectAtRoot(GameObjectNames.SceneCanvas).SetActive(true);
        }

    }
}
