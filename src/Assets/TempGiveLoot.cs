using UnityEngine;
using UnityEngine.Networking;

public class TempGiveLoot : Interactable
{
    public override void OnFocus()
    {
        Debug.Log($"Interactable '{gameObject.name}' gained focus");

        var translation = GameManager.Instance.Localizer.Translate("ui.interact.shop");
        var interactionKey = GameManager.Instance.InputMappings.Interact.ToString();

        _interactionBubble.text = string.Format(translation, interactionKey);
        _interactionBubble.gameObject.SetActive(true);
    }

    public override void OnInteract(NetworkInstanceId playerNetId)
    {
        var player = NetworkServer.FindLocalObject(playerNetId);
        player.GetComponent<PlayerInventory>().Add(GameManager.Instance.ResultFactory.GetLootDrop());
    }

    public override void OnBlur()
    {
        Debug.Log($"Interactable '{gameObject.name}' lost focus");
        _interactionBubble.gameObject.SetActive(false);
    }

}
