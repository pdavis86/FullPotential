﻿using FullPotential.Api.Gameplay.Behaviours;
using FullPotential.Api.Ioc;
using FullPotential.Api.Localization;
using FullPotential.Core.GameManagement;
using FullPotential.Core.Player;
using Unity.Netcode;
using UnityEngine.InputSystem;

// ReSharper disable ClassNeverInstantiated.Global

namespace FullPotential.Core.Environment
{
    public class LootInteractable : Interactable
    {
        public string UnclaimedLootId;

        private ILocalizer _localizer;

        // ReSharper disable once UnusedMember.Local
        private void Awake()
        {
            RequiresServerCheck = false;

            _localizer = DependenciesContext.Dependencies.GetService<ILocalizer>();
        }

        public override void OnFocus()
        {
            if (_interactionBubble == null)
            {
                return;
            }

            var translation = _localizer.Translate("ui.interact.loot");
            var interactInputName = GameManager.Instance.InputActions.Player.Interact.GetBindingDisplayString().ToUpper();
            _interactionBubble.text = string.Format(translation, interactInputName);
            _interactionBubble.gameObject.SetActive(true);
        }

        public override void OnInteract(NetworkObject networkObject)
        {
            networkObject.GetComponent<PlayerBehaviour>().ClaimLootServerRpc(UnclaimedLootId);
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
