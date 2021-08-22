﻿using System;
using UnityEngine;

// ReSharper disable CheckNamespace
// ReSharper disable UnusedMember.Global
// ReSharper disable UnusedMember.Local
// ReSharper disable ClassNeverInstantiated.Global
// ReSharper disable UnusedType.Global
// ReSharper disable MemberCanBePrivate.Global
// ReSharper disable UnassignedField.Global
// ReSharper disable InconsistentNaming

public abstract class Interactable : MonoBehaviour
{
    public float Radius = 3f;

    protected TMPro.TextMeshProUGUI _interactionBubble;

    void Start()
    {
        _interactionBubble = GameManager.Instance.MainCanvasObjects.InteractionBubble.GetComponent<TMPro.TextMeshProUGUI>();
    }

    void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(transform.position, Radius);
    }

    public abstract void OnFocus();

    public abstract void OnInteract(ulong playerNetId);

    public abstract void OnBlur();
}
