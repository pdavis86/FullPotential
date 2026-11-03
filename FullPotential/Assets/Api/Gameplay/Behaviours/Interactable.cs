using FullPotential.Api.GameManagement;
using FullPotential.Api.Ioc;

using Unity.Netcode;

using UnityEngine;


namespace FullPotential.Api.Gameplay.Behaviours
{
    public abstract class Interactable : MonoBehaviour
    {
        public float Radius = 3f;
        public bool RequiresServerCheck = true;

        protected TMPro.TextMeshProUGUI _interactionBubble;

        private void Start()
        {
            //Does not work in Awake, maybe because the class is abstract?
            var gameManager = DependenciesContext.Dependencies.GetService<IGameManager>();
            _interactionBubble = gameManager.GetUserInterface().InteractionBubbleOverlay.GetComponent<TMPro.TextMeshProUGUI>();
        }

        private void OnDrawGizmosSelected()
        {
            Gizmos.color = Color.yellow;
            Gizmos.DrawWireSphere(transform.position, Radius);
        }

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