using FullPotential.Api.Unity.Helpers;
using FullPotential.Core.GameManagement;
using FullPotential.Core.GameManagement.Constants;
using FullPotential.Core.Localization;
using FullPotential.Core.PlayerBehaviours;
using FullPotential.Core.Utilities.UtilityBehaviours;
using Unity.Netcode;
using UnityEngine.InputSystem;

// ReSharper disable once UnusedType.Global

namespace FullPotential.Standard.Scenes.Behaviours
{
    public class ShopInteractable : Interactable
    {
        public override void OnFocus()
        {
            var translation = GameManager.Instance.GetService<Localizer>().Translate("ui.interact.shop");
            var interactInputName = GameManager.Instance.InputActions.Player.Interact.GetBindingDisplayString();
            _interactionBubble.text = string.Format(translation, interactInputName);
            _interactionBubble.gameObject.SetActive(true);
        }

        public override void OnInteract(NetworkObject networkObject)
        {
            //networkObject.GetComponent<PlayerActions>().ClaimLootServerRpc("justgimmieloot");

            var shopUiGameObject = GameObjectHelper.GetObjectAtRoot(GameObjectNames.SceneCanvas).transform.Find("ShopUi").gameObject;

            GameManager.Instance.MainCanvasObjects.OpenCustomMenu(shopUiGameObject);
        }

        public override void OnBlur()
        {
            _interactionBubble.gameObject.SetActive(false);
        }

    }
}
