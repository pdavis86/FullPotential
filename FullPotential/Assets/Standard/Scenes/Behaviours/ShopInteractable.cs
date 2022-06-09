using FullPotential.Api.GameManagement;
using FullPotential.Api.GameManagement.Constants;
using FullPotential.Api.Gameplay.Behaviours;
using FullPotential.Api.Localization;
using FullPotential.Api.Unity.Helpers;
using FullPotential.Api.Utilities;
using Unity.Netcode;
using UnityEngine.InputSystem;

// ReSharper disable once UnusedType.Global

namespace FullPotential.Standard.Scenes.Behaviours
{
    public class ShopInteractable : Interactable
    {
        private IGameManager _gameManager;

        // ReSharper disable once UnusedMember.Local
        private void Awake()
        {
            _gameManager = ModHelper.GetGameManager();
        }

        public override void OnFocus()
        {
            var translation = _gameManager.GetService<ILocalizer>().Translate("ui.interact.shop");
            var interactInputName = _gameManager.InputActions.Player.Interact.GetBindingDisplayString();
            _interactionBubble.text = string.Format(translation, interactInputName);
            _interactionBubble.gameObject.SetActive(true);
        }

        public override void OnInteract(NetworkObject networkObject)
        {
            //networkObject.GetComponent<PlayerActions>().ClaimLootServerRpc("justgimmieloot");

            var shopUiGameObject = GameObjectHelper.GetObjectAtRoot(GameObjectNames.SceneCanvas).transform.Find("ShopUi").gameObject;

            _gameManager.GetUserInterface().OpenCustomMenu(shopUiGameObject);
        }

        public override void OnBlur()
        {
            _interactionBubble.gameObject.SetActive(false);
        }

    }
}
