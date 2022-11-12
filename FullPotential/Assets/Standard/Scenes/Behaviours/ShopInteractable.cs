using FullPotential.Api.GameManagement;
using FullPotential.Api.Gameplay.Behaviours;
using FullPotential.Api.Ioc;
using FullPotential.Api.Localization;
using FullPotential.Api.Unity.Helpers;
using FullPotential.Api.Utilities;
using Unity.Netcode;
using UnityEngine.InputSystem;

// ReSharper disable UnusedType.Global

namespace FullPotential.Standard.Scenes.Behaviours
{
    public class ShopInteractable : Interactable
    {
        private IGameManager _gameManager;

        // ReSharper disable once UnusedMember.Local
        private void Awake()
        {
            _gameManager = DependenciesContext.Dependencies.GetService<IModHelper>().GetGameManager();

            RequiresServerCheck = false;
        }

        public override void OnFocus()
        {
            var translation = DependenciesContext.Dependencies.GetService<ILocalizer>().Translate("ui.interact.shop");
            var interactInputName = _gameManager.InputActions.Player.Interact.GetBindingDisplayString();
            _interactionBubble.text = string.Format(translation, interactInputName);
            _interactionBubble.gameObject.SetActive(true);
        }

        public override void OnInteract(NetworkObject networkObject)
        {
            var shopUiGameObject = GameObjectHelper.GetObjectAtRoot(GameObjectNames.SceneCanvas).transform.Find("ShopUi").gameObject;
            _gameManager.GetUserInterface().OpenCustomMenu(shopUiGameObject);
        }

        public override void OnBlur()
        {
            _interactionBubble.gameObject.SetActive(false);
        }

    }
}
