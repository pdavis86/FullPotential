﻿using UnityEngine;
using UnityEngine.EventSystems;

// ReSharper disable once CheckNamespace
// ReSharper disable UnusedMember.Global
// ReSharper disable UnusedMember.Local
// ReSharper disable ClassNeverInstantiated.Global
// ReSharper disable UnusedType.Global
// ReSharper disable MemberCanBePrivate.Global
// ReSharper disable UnassignedField.Compiler

//todo: rename as this is a generic tooltip script now
public class CraftingTooltip : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler
{
    public delegate void PointerEnterEventDelegate(PointerEventData eventData);
    public event PointerEnterEventDelegate OnPointerEnterForTooltip;

    public void OnPointerEnter(PointerEventData eventData)
    {
        OnPointerEnterForTooltip?.Invoke(eventData);
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        Tooltips.HideTooltip();
    }

}