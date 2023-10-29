using System.Linq;
using FullPotential.Api.Unity.Services;
using UnityEngine;

// ReSharper disable once ClassNeverInstantiated.Global

namespace FullPotential.Core.Unity.Services
{
    public class UnityHelperUtilities : IUnityHelperUtilities
    {
        public GameObject GetObjectAtRoot(string name)
        {
            return UnityEngine.SceneManagement.SceneManager.GetActiveScene().GetRootGameObjects().FirstOrDefault(x => x.name == name);
        }
    }
}
