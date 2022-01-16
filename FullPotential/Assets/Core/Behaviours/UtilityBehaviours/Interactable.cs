using FullPotential.Core.Behaviours.GameManagement;
using Unity.Netcode;
using UnityEngine;

// ReSharper disable UnusedMember.Global
// ReSharper disable ClassNeverInstantiated.Global
// ReSharper disable UnusedType.Global
// ReSharper disable MemberCanBePrivate.Global
// ReSharper disable UnassignedField.Global
// ReSharper disable InconsistentNaming

namespace FullPotential.Core.Behaviours.UtilityBehaviours
{
    public abstract class Interactable : MonoBehaviour
    {
        public float Radius = 3f;
        public bool RequiresServerCheck = true;

        protected TMPro.TextMeshProUGUI _interactionBubble;

        // ReSharper disable once UnusedMember.Local
        private void Awake()
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