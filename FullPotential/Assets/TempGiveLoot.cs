using MLAPI;

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

    public override void OnInteract(ulong playerNetId)
    {
        var loot = GameManager.Instance.ResultFactory.GetLootDrop();
        var playerObj = NetworkManager.Singleton.ConnectedClients[playerNetId].PlayerObject;
        playerObj.GetComponent<PlayerState>().Inventory.Add(loot);
    }

    public override void OnBlur()
    {
        //Debug.Log($"Interactable '{gameObject.name}' lost focus");
        _interactionBubble.gameObject.SetActive(false);
    }

}
