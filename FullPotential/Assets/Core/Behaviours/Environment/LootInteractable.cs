﻿using FullPotential.Core.Behaviours.GameManagement;
using FullPotential.Core.Behaviours.PlayerBehaviours;
using FullPotential.Core.Behaviours.UtilityBehaviours;
using UnityEngine.InputSystem;

// ReSharper disable UnusedType.Global
// ReSharper disable ClassNeverInstantiated.Global
// ReSharper disable UnusedMember.Local

namespace FullPotential.Core.Behaviours.Environment
{
    public class LootInteractable : Interactable
    {
        public string UnclaimedLootId;

        private void Awake()
        {
            RequiresServerCheck = false;
        }

        public override void OnFocus()
        {
            if (_interactionBubble == null)
            {
                return;
            }
            var translation = GameManager.Instance.Localizer.Translate("ui.interact.loot");
            var interactInputName = GameManager.Instance.InputActions.Player.Interact.GetBindingDisplayString().ToUpper();
            _interactionBubble.text = string.Format(translation, interactInputName);
            _interactionBubble.gameObject.SetActive(true);
        }

        public override void OnInteract(PlayerState playerState)
        {
            playerState.ClaimLootServerRpc(UnclaimedLootId);
            Destroy(gameObject);
        }

        public override void OnBlur()
        {
            if (_interactionBubble != null)
            {
                _interactionBubble.gameObject.SetActive(false);
            }
        }

    }
}
