﻿using FullPotential.Core.Behaviours.GameManagement;
using FullPotential.Core.Behaviours.PlayerBehaviours;
using FullPotential.Core.Behaviours.UtilityBehaviours;
using Unity.Netcode;
using UnityEngine.InputSystem;

// ReSharper disable once UnusedType.Global

namespace FullPotential.Core.Behaviours.Environment
{
    public class ShopInteractable : Interactable
    {
        public override void OnFocus()
        {
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
            _interactionBubble.gameObject.SetActive(false);
        }

    }
}
