using UnityEngine;

// ReSharper disable UnusedType.Global

namespace FullPotential.Core.Utilities.UtilityBehaviours
{
    public class DestroyOnTriggerEnter : MonoBehaviour
    {
#pragma warning disable 0649
        [SerializeReference] private GameObject _gameObjectToDestroy;
#pragma warning restore 0649

        // ReSharper disable once UnusedMember.Local
        private void OnTriggerEnter()
        {
            Destroy(_gameObjectToDestroy);
        }
    }
}
