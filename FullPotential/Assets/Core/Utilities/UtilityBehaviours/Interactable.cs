using FullPotential.Core.GameManagement;
using Unity.Netcode;
using UnityEngine;

// ReSharper disable ClassNeverInstantiated.Global

namespace FullPotential.Core.Utilities.UtilityBehaviours
{
    public abstract class Interactable : MonoBehaviour
    {
        public float Radius = 3f;
        public bool RequiresServerCheck = true;

        // ReSharper disable once InconsistentNaming
        protected TMPro.TextMeshProUGUI _interactionBubble;

        // ReSharper disable once UnusedMember.Local
        private void Start()
        {
            _interactionBubble = GameManager.Instance.MainCanvasObjects.InteractionBubble.GetComponent<TMPro.TextMeshProUGUI>();
        }

        // ReSharper disable once UnusedMember.Local
        private void OnDrawGizmosSelected()
        {
            Gizmos.color = Color.yellow;
            Gizmos.DrawWireSphere(transform.position, Radius);
        }

        // ReSharper disable once UnusedMember.Local
        private void OnDisable()
        {
            if (_interactionBubble != null)
            {
                OnBlur();
            }
        }

        public abstract void OnFocus();

        public abstract void OnInteract(NetworkObject networkObject);

        public abstract void OnBlur();
    }
}