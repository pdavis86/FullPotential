﻿using FullPotential.Core.GameManagement;
using FullPotential.Core.Localization;
using FullPotential.Core.PlayerBehaviours;
using FullPotential.Core.Utilities.UtilityBehaviours;
using Unity.Netcode;
using UnityEngine.InputSystem;

// ReSharper disable ClassNeverInstantiated.Global

namespace FullPotential.Core.Environment
{
    public class LootInteractable : Interactable
    {
        public string UnclaimedLootId;

        // ReSharper disable once UnusedMember.Local
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
            var translation = GameManager.Instance.GetService<Localizer>().Translate("ui.interact.loot");
            var interactInputName = GameManager.Instance.InputActions.Player.Interact.GetBindingDisplayString().ToUpper();
            _interactionBubble.text = string.Format(translation, interactInputName);
            _interactionBubble.gameObject.SetActive(true);
        }

        public override void OnInteract(NetworkObject networkObject)
        {
            networkObject.GetComponent<PlayerActions>().ClaimLootServerRpc(UnclaimedLootId);
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