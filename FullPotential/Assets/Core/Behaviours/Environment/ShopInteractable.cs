using FullPotential.Core.Behaviours.GameManagement;
using FullPotential.Core.Behaviours.PlayerBehaviours;
using FullPotential.Core.Behaviours.UtilityBehaviours;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.InputSystem;

// ReSharper disable UnusedType.Global

namespace FullPotential.Core.Behaviours.Environment
{
    public class ShopInteractable : Interactable
    {
        public override void OnFocus()
        {
            //Debug.Log($"Interactable '{gameObject.name}' gained focus");

            var translation = GameManager.Instance.Localizer.Translate("ui.interact.shop");
            var interactInputName = GameManager.Instance.InputActions.Player.Interact.GetBindingDisplayString();
            _interactionBubble.text = string.Format(translation, interactInputName);
            _interactionBubble.gameObject.SetActive(true);
        }

        public override void OnInteract(NetworkObject networkObject)
        {
            networkObject.GetComponent<PlayerActions>().ClaimLootServerRpc("justgimmieloot");
        }

        public override void OnBlur()
        {
            //Debug.Log($"Interactable '{gameObject.name}' lost focus");
            _interactionBubble.gameObject.SetActive(false);
        }

    }
}
