﻿using System.Linq;
using UnityEngine;

namespace FullPotential.Core.Helpers
{
    public static class UnityHelper
    {
        //public static bool IsPointerOverUIElement(List<RaycastResult> eventSystemRaysastResults)
        //{
        //    var uiLayer = LayerMask.NameToLayer("UI");
        //    return GetEventSystemRaycastResults().FirstOrDefault(x => x.gameObject.layer == uiLayer).gameObject != null;
        //}

        //static List<RaycastResult> GetEventSystemRaycastResults()
        //{
        //    var raycastResults = new List<RaycastResult>();
        //    EventSystem.current.RaycastAll(
        //        new PointerEventData(EventSystem.current)
        //        {
        //            position = Input.mousePosition
        //        }, raycastResults
        //    );
        //    return raycastResults;
        //}

    }
}
