using MLAPI;
using UnityEngine.InputSystem;

// ReSharper disable CheckNamespace
// ReSharper disable UnusedType.Global

public class LootInteractable : Interactable
{
    public override void OnFocus()
    {
        //Debug.Log($"Interactable '{gameObject.name}' gained focus");

        var translation = GameManager.Instance.Localizer.Translate("ui.interact.loot");
        var interactInputName = GameManager.Instance.InputActions.Player.Interact.GetBindingDisplayString();
        _interactionBubble.text = string.Format(translation, interactInputName);
        _interactionBubble.gameObject.SetActive(true);
    }

    public override void OnInteract(ulong playerNetId)
    {
        var loot = GameManager.Instance.ResultFactory.GetLootDrop();
        var playerObj = NetworkManager.Singleton.ConnectedClients[playerNetId].PlayerObject;
        playerObj.GetComponent<PlayerState>().AddToInventory(loot);
    }

    public override void OnBlur()
    {
        //Debug.Log($"Interactable '{gameObject.name}' lost focus");
        _interactionBubble.gameObject.SetActive(false);
    }

}
