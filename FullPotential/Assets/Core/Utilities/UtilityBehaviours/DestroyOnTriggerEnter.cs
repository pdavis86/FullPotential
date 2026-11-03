using UnityEngine;


namespace FullPotential.Core.Utilities.UtilityBehaviours
{
    public class DestroyOnTriggerEnter : MonoBehaviour
    {
#pragma warning disable 0649
        [SerializeReference] private GameObject _gameObjectToDestroy;
#pragma warning restore 0649

        private void OnTriggerEnter(Collider other)
        {
            Destroy(_gameObjectToDestroy);
        }
    }
}
