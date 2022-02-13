using UnityEngine;

// ReSharper disable once UnusedType.Global

namespace FullPotential.Core.Utilities.UtilityBehaviours
{
    public class DestroyOnTriggerEnter : MonoBehaviour
    {
#pragma warning disable CS0649
        [SerializeReference] private GameObject _gameObjectToDestroy;
#pragma warning restore CS0649

        // ReSharper disable once UnusedMember.Local
        private void OnTriggerEnter()
        {
            Destroy(_gameObjectToDestroy);
        }
    }
}
