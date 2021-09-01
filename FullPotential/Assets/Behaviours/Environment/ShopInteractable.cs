﻿using System;
using UnityEngine.InputSystem;

public class ShopInteractable : Interactable
{
    public override void OnFocus()
    {
        //Debug.Log($"Interactable '{gameObject.name}' gained focus");

        var translation = GameManager.Instance.Localizer.Translate("ui.interact.shop");
        var interactInputName = GameManager.Instance.InputActions.Player.Interact.GetBindingDisplayString();
        _interactionBubble.text = string.Format(translation, interactInputName);
        _interactionBubble.gameObject.SetActive(true);
    }

    public override void OnInteract(ulong playerNetId)
    {
        throw new NotImplementedException();
    }

    public override void OnBlur()
    {
        //Debug.Log($"Interactable '{gameObject.name}' lost focus");
        _interactionBubble.gameObject.SetActive(false);
    }

}
