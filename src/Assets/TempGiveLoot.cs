using UnityEngine;
using UnityEngine.Networking;

public class TempGiveLoot : Interactable
{
    public override void OnFocus()
    {
        //Debug.Log($"Interactable '{gameObject.name}' gained focus");

        var translation = GameManager.Instance.Localizer.Translate("ui.interact.shop");

        //todo: get it from UnityEngine.InputSystem.;

        var interactionKey = "E"; // GameManager.Instance.InputMappings.Interact.ToString();

        _interactionBubble.text = string.Format(translation, interactionKey);
        _interactionBubble.gameObject.SetActive(true);
    }

    public override void OnInteract(NetworkInstanceId playerNetId)
    {
        var playerObj = NetworkServer.FindLocalObject(playerNetId);
        var loot = GameManager.Instance.ResultFactory.GetLootDrop(playerObj);
        playerObj.GetComponent<PlayerInventory>().Add(loot);
    }

    public override void OnBlur()
    {
        //Debug.Log($"Interactable '{gameObject.name}' lost focus");
        _interactionBubble.gameObject.SetActive(false);
    }

}
