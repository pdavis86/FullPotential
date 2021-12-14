using FullPotential.Core.Behaviours.GameManagement;
using FullPotential.Core.Behaviours.PlayerBehaviours;
using FullPotential.Core.Behaviours.UtilityBehaviours;
using Unity.Netcode;
using UnityEngine.InputSystem;

// ReSharper disable UnusedType.Global
// ReSharper disable once ClassNeverInstantiated.Global

namespace FullPotential.Core.Behaviours.Environment
{
    public class LootInteractable : Interactable
    {
        public string UnclaimedLootId;

        public override void OnFocus()
        {
            var translation = GameManager.Instance.Localizer.Translate("ui.interact.loot");
            var interactInputName = GameManager.Instance.InputActions.Player.Interact.GetBindingDisplayString().ToUpper();
            _interactionBubble.text = string.Format(translation, interactInputName);
            _interactionBubble.gameObject.SetActive(true);
        }

        public override void OnInteract(ulong playerNetId)
        {
            var playerObj = NetworkManager.Singleton.ConnectedClients[playerNetId].PlayerObject;
            playerObj.GetComponent<PlayerState>().ClaimLootServerRpc(UnclaimedLootId);
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
